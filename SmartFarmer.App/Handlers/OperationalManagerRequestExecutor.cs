using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Handlers.Providers;
using SmartFarmer.Misc;
using SmartFarmer.OperationalManagement;
using SmartFarmer.Position;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Utils;

namespace SmartFarmer.Handlers;

public class OperationalManagerRequestExecutor
{
    private readonly IFarmerDeviceKindProvider _deviceProvider;
    private readonly IFarmerConfigurationProvider _configProvider;
    private readonly IFarmerAppCommunicationHandler _communicationHandler;
    private readonly IFarmerLocalInformationManager _localInfoManager;

    private Dictionary<string, FarmerGardenHubHandler> _hubHandlers;
    private CancellationTokenSource _currentOperationTokenSource;

    public OperationalManagerRequestExecutor()
    {
        _hubHandlers = new Dictionary<string, FarmerGardenHubHandler>();        
        _currentOperationTokenSource = new CancellationTokenSource();

        _deviceProvider = FarmerServiceLocator.GetService<IFarmerDeviceKindProvider>(true);
        _configProvider = FarmerServiceLocator.GetService<IFarmerConfigurationProvider>(true);
        _communicationHandler = FarmerServiceLocator.GetService<IFarmerAppCommunicationHandler>(true);
        _localInfoManager = FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true);

        SubscribeToCommunication();
    }

    public void Execute(object sender, OperationRequestEventArgs e)
    {
        SmartFarmerLog.Information($"Operation {e.Operation} requested by {e.Sender.Name}");
        var opManager = sender as IOperationalModeManager;

        var operationToken = _currentOperationTokenSource.Token;

        switch(e.Operation)
        {
            case AppOperation.RunPlan:
                {
                    Task.Run(async () => 
                    {
                        await ExecutePlanAsync(
                            e.AdditionalData.FirstOrDefault(),
                            opManager,
                            e,
                            operationToken);
                    });
                }

                break;
            
            case AppOperation.RunAutoIrrigationPlan:
                {
                    Task.Run(async () => 
                    {
                        await ExecuteAutoIrrigationPlanAsync(
                            e.AdditionalData.FirstOrDefault(),
                            opManager,
                            e,
                            operationToken);
                    });
                }

                break;

            case AppOperation.UpdateAllGardens:
                Task.Run(async () => 
                    {
                        await _localInfoManager.ReinitializeGardensAsync(_currentOperationTokenSource.Token);
                    });
                break;

            case AppOperation.MarkAlert:
                Task.Run(async () => 
                    await InvertAlertReadStatusAsync(
                        e.AdditionalData.FirstOrDefault(),
                        operationToken));
                break;
            
            case AppOperation.CliCommand:
                Task.Run(async () => 
                    await SendCliCommandAsync(
                        e.AdditionalData.FirstOrDefault(), 
                        e.AdditionalData.LastOrDefault(),
                        opManager,
                        e,
                        operationToken));
                break;

            case AppOperation.TestPosition:
                Task.Run(async () => await SendTestPosition(e.AdditionalData.FirstOrDefault(), operationToken));
                break;

            case AppOperation.MoveToPosition:
                Task.Run(async () => 
                {
                    var result = await MoveToPosition(
                        e.AdditionalData.FirstOrDefault(), 
                        e.AdditionalData.LastOrDefault(),
                        opManager,
                        e,
                        operationToken);
                });
                break;

            case AppOperation.StopCurrentOperation:
                _currentOperationTokenSource.Cancel();
                break;

            default: 
                throw new NotSupportedException();
        }
    }

    private void SubscribeToCommunication()
    {
        if (_communicationHandler == null) return;

        _communicationHandler.LocalGardenAdded += LocalGardenAdded;
        _communicationHandler.LocalGardenRemoved += LocalGardenRemoved;
    }

    private void LocalGardenAdded(object sender, GardenChangedEventArgs e)
    {
        Task.Run(async () => await InitializeHubForGardenAsync(e.GardenId, CancellationToken.None));
    }

    private void LocalGardenRemoved(object sender, GardenChangedEventArgs e)
    {
        RemoveHubForGarden(e.GardenId);
    }

    private void RemoveHubForGarden(string gardenId)
    {
        if (!_hubHandlers.ContainsKey(gardenId))
        {
            return;
        }

        var closingHub = _hubHandlers[gardenId];

        closingHub.CliCommandResultReceived -= CliCommandResultReceived;

        if (closingHub is IDisposable disp)
        {
            disp.Dispose();
        }

        _hubHandlers.Remove(gardenId);
    }

    private async Task InitializeHubForGardenAsync(string gardenId, CancellationToken token)
    {
        if (_hubHandlers.ContainsKey(gardenId))
        {
            RemoveHubForGarden(gardenId);
        }

        var garden = _localInfoManager.Gardens.FirstOrDefault(x => x.Key == gardenId).Value;
        if (garden == null) return;

        var hub = new FarmerGardenHubHandler(garden, _configProvider.GetHubConfiguration());
        await hub.InitializeAsync(token);

        hub.CliCommandResultReceived += CliCommandResultReceived;

        _hubHandlers.TryAdd(gardenId, hub);
    }

    private void CliCommandResultReceived(object sender, CliCommandResultEventArgs args)
    {
        SmartFarmerLog.Debug($"cli result: {args.Result}");
    }

    private async Task SendCliCommandAsync(
        string gardenId, 
        string command, 
        IOperationalModeManager opManager,
        OperationRequestEventArgs args,
        CancellationToken token)
    {
        await _hubHandlers[gardenId]?.SendCliCommandAsync(gardenId, command, token);
    }

    private async Task<bool> MoveToPosition(
        string gardenId, 
        string serializedPosition, 
        IOperationalModeManager opManager,
        OperationRequestEventArgs args,
        CancellationToken token)
    {
        var position = serializedPosition.Deserialize<Farmer5dPoint>();
        if (position == null) 
        {
            SmartFarmerLog.Error($"{serializedPosition} is not a valid point");
            
            args.Result = "Invalid destination position";
            opManager?.ProcessResult(args);

            return false;
        }

        var device = _deviceProvider.GetDeviceManager(gardenId);
        if (device == null)
        {
            SmartFarmerLog.Error($"no valid device found for garden {gardenId}");

            args.Result = $"No device found for garden {gardenId}";
            opManager?.ProcessResult(args);

            return false;
        }

        try
        {
            var result = await device.MoveToPosition(position, token);
            
            var suffix = result ? "successfully" : "with errors";

            args.Result = $"moved finished {suffix}";
            opManager?.ProcessResult(args);
        }
        catch(AggregateException ex)
        {
            SmartFarmerLog.Exception(ex);

            args.ExecutionException = ex.InnerException;
            opManager?.ProcessResult(args);
        }
        catch (TaskCanceledException ex)
        {
            SmartFarmerLog.Exception(ex);

            args.ExecutionException = ex;
            opManager?.ProcessResult(args);
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);

            args.ExecutionException = ex;
            opManager?.ProcessResult(args);
        }

        return false;
    }

    private async Task SendTestPosition(string gardenId, CancellationToken token)
    {
        await _hubHandlers[gardenId].SendDevicePosition(new Movement.FarmerDevicePositionRequestData()
        {
            GardenId = gardenId,
            PositionDt = DateTime.UtcNow,
            Position = new Farmer5dPoint(1, 2, 3, 4, 5)
        },
        token);

        await Task.Delay(1000);

        await _hubHandlers[gardenId].SendDevicePosition(new Movement.FarmerDevicePositionRequestData()
        {
            GardenId = gardenId,
            PositionDt = DateTime.UtcNow,
            Position = new Farmer5dPoint(1, 2, 3, 4, 15)
        },
        token);
    }

    private async Task<bool> InvertAlertReadStatusAsync(string alertId, CancellationToken token)
    {
        var garden = GardenUtils.GetGardenByAlert(alertId) as FarmerGarden;
        if (garden == null) return false;

        var alert = garden.Alerts.First(x => x.ID == alertId);
        var newState = !alert.MarkedAsRead;
        
        return await MarkAlertAsReadAsync(garden.ID, alertId, newState, token);
    }

    private async Task ExecutePlanAsync(
        string planId, 
        IOperationalModeManager opManager,
        OperationRequestEventArgs args,
        CancellationToken token)
    {
        var garden = GardenUtils.GetGardenByPlan(planId);
        if (garden == null || garden is not FarmerGarden fGarden)
        {
            SmartFarmerLog.Error("No valid garden found for plan " + planId);

            args.Result = "invalid garden for plan";
            opManager?.ProcessResult(args);

            return;
        }

        try
        {
            var result = await fGarden.ExecutePlan(planId, token);

            var suffix = result ? "successfully" : "with errors";

            args.Result = $"plan {planId} executed {suffix}";
            opManager?.ProcessResult(args);
        }
        catch (AggregateException ex)
        {
            args.ExecutionException = ex.InnerException;
            opManager?.ProcessResult(args);
            return;
        }
        catch (Exception ex)
        {
            args.ExecutionException = ex;
            opManager?.ProcessResult(args);
            return;
        }
    }

    private async Task ExecuteAutoIrrigationPlanAsync(
        string gardenId,
        IOperationalModeManager opManager,
        OperationRequestEventArgs args,
        CancellationToken token)
    {
        if (!_localInfoManager.Gardens.ContainsKey(gardenId))
        {
            args.Result = "Invalid garden";
            args.IsError = true;
            opManager?.ProcessResult(args);
            return;
        }

        var garden = _localInfoManager.Gardens[gardenId];

        if (string.IsNullOrEmpty(garden.IrrigationPlanId))
        {
            args.Result = "Invalid irrigation plan";
            args.IsError = true;
            opManager?.ProcessResult(args);
            return;
        }

        await ExecutePlanAsync(
            garden.IrrigationPlanId, 
            opManager, 
            args, 
            token);
    }

    private async Task ExecutePlansAsync(string[] planIds, CancellationToken token)
    {
        var tasks = new List<Task>();

        foreach (var planId in planIds)
        {
            var garden = GardenUtils.GetGardenByPlan(planId) as FarmerGarden;

            if (garden == null) continue;
            var plan = garden.Plans.First(x => x.ID == planId) as IFarmerPlan;

            if (plan == null) continue;

            tasks.Add(
                Task.Run(async () => {
                    try
                    {
                        await plan.Execute(token);
                    }
                    catch (TaskCanceledException taskCanceled)
                    {
                        SmartFarmerLog.Exception(taskCanceled);
                    }
                    catch (Exception ex)
                    {
                        SmartFarmerLog.Exception(ex);
                        SmartFarmerLog.Exception(plan.LastException);
                    }
                    finally
                    {
                        SmartFarmerLog.Information("stopping plan \"" + plan.Name + "\"");
                    }
                }));
        }

        try {  
            await Task.WhenAll(tasks);  
        } 
        catch {}
    }

    private void ClearLocalData()
    {
        _localInfoManager.ClearLocalData(true, false, false);
    }

    private async Task<bool> MarkAlertAsReadAsync(string gardenId, string alertId, bool status, CancellationToken token)
    {
        return await FarmerServiceLocator.GetService<IFarmerAlertHandler>(true, gardenId).MarkAlertAsReadAsync(alertId, status, token);
    }

}