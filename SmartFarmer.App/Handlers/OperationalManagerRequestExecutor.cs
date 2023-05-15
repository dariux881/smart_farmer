using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.Handlers.Providers;
using SmartFarmer.Misc;
using SmartFarmer.OperationalManagement;
using SmartFarmer.Position;
using SmartFarmer.Utils;

namespace SmartFarmer.Handlers;

public class OperationalManagerRequestExecutor
{
    private readonly IFarmerDeviceKindProvider _deviceProvider;
    private readonly IFarmerConfigurationProvider _configProvider;
    private readonly IFarmerAppCommunicationHandler _communicationHandler;
    private readonly IFarmerLocalInformationManager _localInfoManager;

    private Dictionary<string, FarmerGroundHubHandler> _hubHandlers;
    private CancellationTokenSource _currentOperationTokenSource;

    public OperationalManagerRequestExecutor()
    {
        _hubHandlers = new Dictionary<string, FarmerGroundHubHandler>();        
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

            case AppOperation.UpdateAllGrounds:
                Task.Run(async () => 
                    {
                        await _localInfoManager.ReinitializeGroundsAsync(_currentOperationTokenSource.Token);
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

        _communicationHandler.LocalGroundAdded += LocalGroundAdded;
        _communicationHandler.LocalGroundRemoved += LocalGroundRemoved;
    }

    private void LocalGroundAdded(object sender, GroundChangedEventArgs e)
    {
        Task.Run(async () => await InitializeHubForGroundAsync(e.GroundId, CancellationToken.None));
    }

    private void LocalGroundRemoved(object sender, GroundChangedEventArgs e)
    {
        RemoveHubForGround(e.GroundId);
    }

    private void RemoveHubForGround(string groundId)
    {
        if (!_hubHandlers.ContainsKey(groundId))
        {
            return;
        }

        var closingHub = _hubHandlers[groundId];

        closingHub.CliCommandResultReceived -= CliCommandResultReceived;

        if (closingHub is IDisposable disp)
        {
            disp.Dispose();
        }

        _hubHandlers.Remove(groundId);
    }

    private async Task InitializeHubForGroundAsync(string groundId, CancellationToken token)
    {
        if (_hubHandlers.ContainsKey(groundId))
        {
            RemoveHubForGround(groundId);
        }

        var ground = _localInfoManager.Grounds.FirstOrDefault(x => x.Key == groundId).Value;
        if (ground == null) return;

        var hub = new FarmerGroundHubHandler(ground, _configProvider.GetHubConfiguration());
        await hub.InitializeAsync(token);

        hub.CliCommandResultReceived += CliCommandResultReceived;

        _hubHandlers.TryAdd(groundId, hub);
    }

    private void CliCommandResultReceived(object sender, CliCommandResultEventArgs args)
    {
        SmartFarmerLog.Debug($"cli result: {args.Result}");
    }

    private async Task SendCliCommandAsync(
        string groundId, 
        string command, 
        IOperationalModeManager opManager,
        OperationRequestEventArgs args,
        CancellationToken token)
    {
        await _hubHandlers[groundId]?.SendCliCommandAsync(groundId, command, token);
    }

    private async Task<bool> MoveToPosition(
        string groundId, 
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

        var device = _deviceProvider.GetDeviceManager(groundId);
        if (device == null)
        {
            SmartFarmerLog.Error($"no valid device found for ground {groundId}");

            args.Result = $"No device found for ground {groundId}";
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

    private async Task SendTestPosition(string groundId, CancellationToken token)
    {
        await _hubHandlers[groundId].SendDevicePosition(new Movement.FarmerDevicePositionRequestData()
        {
            GroundId = groundId,
            PositionDt = DateTime.UtcNow,
            Position = new Farmer5dPoint(1, 2, 3, 4, 5)
        },
        token);

        await Task.Delay(1000);

        await _hubHandlers[groundId].SendDevicePosition(new Movement.FarmerDevicePositionRequestData()
        {
            GroundId = groundId,
            PositionDt = DateTime.UtcNow,
            Position = new Farmer5dPoint(1, 2, 3, 4, 15)
        },
        token);
    }

    private async Task<bool> InvertAlertReadStatusAsync(string alertId, CancellationToken token)
    {
        var ground = GroundUtils.GetGroundByAlert(alertId) as FarmerGround;
        if (ground == null) return false;

        var alert = ground.Alerts.First(x => x.ID == alertId);
        var newState = !alert.MarkedAsRead;
        
        return await MarkAlertAsReadAsync(ground.ID, alertId, newState, token);
    }

    private static async Task ExecutePlanAsync(
        string planId, 
        IOperationalModeManager opManager,
        OperationRequestEventArgs args,
        CancellationToken token)
    {
        var ground = GroundUtils.GetGroundByPlan(planId);
        if (ground == null || ground is not FarmerGround fGround)
        {
            SmartFarmerLog.Error("No valid ground found for plan " + planId);

            args.Result = "invalid ground for plan";
            opManager?.ProcessResult(args);

            return;
        }

        try
        {
            var result = await fGround.ExecutePlan(planId, token);

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

    private static async Task ExecutePlansAsync(string[] planIds, CancellationToken token)
    {
        var tasks = new List<Task>();

        foreach (var planId in planIds)
        {
            var ground = GroundUtils.GetGroundByPlan(planId) as FarmerGround;

            if (ground == null) continue;
            var plan = ground.Plans.First(x => x.ID == planId);

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

    private async Task<bool> MarkAlertAsReadAsync(string groundId, string alertId, bool status, CancellationToken token)
    {
        return await FarmerServiceLocator.GetService<IFarmerAlertHandler>(true, groundId).MarkAlertAsReadAsync(alertId, status, token);
    }

}