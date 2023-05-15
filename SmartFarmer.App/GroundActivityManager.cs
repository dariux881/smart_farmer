using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Configurations;
using SmartFarmer.Data;
using SmartFarmer.Handlers;
using SmartFarmer.Handlers.Providers;
using SmartFarmer.Misc;
using SmartFarmer.Position;
using SmartFarmer.OperationalManagement;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer;

public class GroundActivityManager
{
    private List<IOperationalModeManager> _operationalManagers;
    private Dictionary<string, FarmerGroundHubHandler> _hubHandlers;
    private IFarmerAppCommunicationHandler _communicationHandler;
    private IFarmerConfigurationProvider _configProvider;
    private IFarmerDeviceKindProvider _deviceProvider;
    private CancellationTokenSource _managementTokenSource;
    private CancellationTokenSource _currentOperationTokenSource;

    public GroundActivityManager()
    {
        _configProvider = FarmerServiceLocator.GetService<IFarmerConfigurationProvider>(true);
        _communicationHandler = FarmerServiceLocator.GetService<IFarmerAppCommunicationHandler>(true);
        _deviceProvider = FarmerServiceLocator.GetService<IFarmerDeviceKindProvider>(true);

        _managementTokenSource = new CancellationTokenSource();
        _currentOperationTokenSource = new CancellationTokenSource();
    }

    public async Task Run()
    {
        _hubHandlers = new Dictionary<string, FarmerGroundHubHandler>();
        var token = _managementTokenSource.Token;

        PrepareEnvironment();

        var loginResult = await Login(token);
        if (!loginResult)
        {
            //TODO retry in case of failure
            SmartFarmerLog.Error($"Login failed for user {_configProvider.GetUserConfiguration().UserName}. Stopping manager");
            return;
        }

        _communicationHandler.NotifyNewLoggedUser();

        FillOperationalManagers();

        await InitializeGroundsAsync(token);
        await InitializeHubsForGroundsAsync(token);

        var tasks = new List<Task>();
        foreach (var opManager in _operationalManagers)
        {
            tasks.Add(Task.Run(() => opManager.Run(token)));
        }

        await Task.WhenAny(tasks);

        foreach (var opManager in _operationalManagers)
        {
            try
            {
                opManager.Dispose();
            }
            catch (Exception ex)
            {
                SmartFarmerLog.Exception(ex);
            }
        }
    }

    private async Task InitializeHubsForGroundsAsync(CancellationToken token)
    {
        var closingHubs = _hubHandlers.Values.Where(x => x is IDisposable).Cast<IDisposable>();

        foreach (var hub in closingHubs)
        {
            hub.Dispose();
        }

        _hubHandlers.Clear();

        foreach(var ground in LocalConfiguration.Grounds)
        {
            var hub = new FarmerGroundHubHandler(ground.Value, _configProvider.GetHubConfiguration());
            await hub.InitializeAsync(token);

            hub.CliCommandResultReceived += (s, e) => SmartFarmerLog.Debug($"cli result: {e.Result}");

            _hubHandlers.TryAdd(ground.Key, hub);
        }
    }

    private async Task<bool> Login(CancellationToken token)
    {
        var user = new Data.Security.LoginRequestData() {
                UserName = _configProvider.GetUserConfiguration()?.UserName, 
                Password = _configProvider.GetUserConfiguration()?.Password
            };

        // login
        var loginResponse = await FarmerRequestHandler.Login(
            user, 
            token);

        // save login result
        if (loginResponse == null || !loginResponse.IsSuccess)
        {
            SmartFarmerLog.Error("Invalid login for user " + user.UserName + " error: " + loginResponse?.ErrorMessage);
            return false;
        }

        LocalConfiguration.LoggedUserId = loginResponse.UserId;
        LocalConfiguration.Token = loginResponse.Token;

        return !string.IsNullOrEmpty(LocalConfiguration.Token);
    }

    private void FillOperationalManagers()
    {
        if (_operationalManagers == null) _operationalManagers = new List<IOperationalModeManager>();

        _operationalManagers.ForEach(opMan => 
            {
                opMan.NewOperationRequired -= ExecuteRequiredOperation;
                opMan.Dispose();
            }
        );

        _operationalManagers.Clear();

        if (_configProvider.GetAppConfiguration().AppOperationalMode == null)
        {
            return;
        }

        if (_configProvider.GetAppConfiguration().AppOperationalMode.Value.HasFlag(AppOperationalMode.Console))
        {
            var console = new ConsoleOperationalModeManager();
            _operationalManagers.Add(console);

            FarmerServiceLocator.MapService<IConsoleOperationalModeManager>(() => console);
        }

        if (_configProvider.GetAppConfiguration().AppOperationalMode.Value.HasFlag(AppOperationalMode.Auto))
        {
            var auto = new AutomaticOperationalManager(_configProvider.GetAppConfiguration());
            _operationalManagers.Add(auto);

            FarmerServiceLocator.MapService<IAutoOperationalModeManager>(() => auto);
        }

        if (_configProvider.GetAppConfiguration().AppOperationalMode.Value.HasFlag(AppOperationalMode.Cli))
        {
            var cli = new CliOperationalManager(_configProvider.GetHubConfiguration());
            _operationalManagers.Add(cli);

            FarmerServiceLocator.MapService<ICliOperationalModeManager>(() => cli);
        }

        _operationalManagers.ForEach(async opMan =>
        {
            await opMan.InitializeAsync(_managementTokenSource.Token);
            opMan.NewOperationRequired += ExecuteRequiredOperation;
        });
    }

    private void ExecuteRequiredOperation(object sender, OperationRequestEventArgs e)
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
                ClearLocalData();
                Task.Run(async () => 
                    {
                        await InitializeGroundsAsync(_managementTokenSource.Token);
                        await InitializeHubsForGroundsAsync(_managementTokenSource.Token);
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
        LocalConfiguration.ClearLocalData(true, false, false);
    }

    private async Task<bool> MarkAlertAsReadAsync(string groundId, string alertId, bool status, CancellationToken token)
    {
        return await FarmerServiceLocator.GetService<IFarmerAlertHandler>(true, groundId).MarkAlertAsReadAsync(alertId, status, token);
    }

    private async Task InitializeGroundsAsync(CancellationToken token)
    {
        // get all grounds
        var grounds = await FarmerRequestHandler.GetGroundsList(token);
        if (grounds == null)
        {
            SmartFarmerLog.Error("invalid grounds");
            return;
        }

        if (!grounds.Any())
        {
            SmartFarmerLog.Debug("no assigned grounds");
            return;
        }

        SmartFarmerLog.Debug(
                "List of grounds:\n\t" + grounds.Select(x => x.ID).Aggregate((g1, g2) => g1 + ", " + g2));

        var locallyInterestedGrounds = 
            _configProvider.GetAppConfiguration().LocalGroundIds != null && _configProvider.GetAppConfiguration().LocalGroundIds.Any() ?
                grounds
                    .Where(x => 
                        _configProvider.GetAppConfiguration().LocalGroundIds.Contains(x.ID)).ToList() :
                new List<IFarmerGround>() { grounds.First() };

        var tasks = new List<Task>();

        foreach (var ground in locallyInterestedGrounds)
        {
            await InitializeServicesForSingleGround(ground, token);

            var groundId = ground.ID;
            tasks.Add(Task.Run(async () => {
                var ground = await FarmerRequestHandler
                    .GetGround(groundId, token);
                
                LocalConfiguration.Grounds.TryAdd(ground.ID, ground);
                _communicationHandler.NotifyNewGround(ground.ID);
            }));
        }

        try {  
           await Task.WhenAll(tasks);
        }  
        catch(AggregateException ae) 
        {
            SmartFarmerLog.Exception(ae);
        }
        catch(Exception ex) 
        {
            SmartFarmerLog.Exception(ex);
        }
    }

    private void PrepareEnvironment()
    {
        // SmartFarmerLog.SetShowThreadInfo(true);

    }
    
    private async Task InitializeServicesForSingleGround(IFarmerGround ground, CancellationToken cancellationToken)
    {
        // clearing possibly old mapped services
        FarmerServiceLocator.RemoveService<IFarmerToolsManager>();
        FarmerServiceLocator.RemoveService<IFarmerAlertHandler>();
        FarmerServiceLocator.RemoveService<IFarmerMoveOnGridTask>();
        FarmerServiceLocator.RemoveService<FarmerMoveOnGridTask>();
        FarmerServiceLocator.RemoveService<IFarmerMoveArmAtHeightTask>();
        FarmerServiceLocator.RemoveService<FarmerMoveArmAtHeightTask>();
        FarmerServiceLocator.RemoveService<IFarmerProvideWaterTask>();
        FarmerServiceLocator.RemoveService<FarmerProvideWaterTask>();

        // preparing new services
        FarmerServiceLocator.MapService<IFarmerToolsManager>(() => new FarmerToolsManager(ground));

        var alertHandler = new FarmerAlertHandler(ground, _configProvider.GetHubConfiguration());
        FarmerServiceLocator.MapService<IFarmerAlertHandler>(() => alertHandler, ground);

        var deviceHandler = _deviceProvider.GetDeviceManager(ground.ID);

        var moveOnGridTask = new FarmerMoveOnGridTask(ground, deviceHandler);
        FarmerServiceLocator.MapService<IFarmerMoveOnGridTask>(() => moveOnGridTask, ground);
        FarmerServiceLocator.MapService<FarmerMoveOnGridTask>(() => moveOnGridTask, ground);

        var moveAtHeightTask = new FarmerMoveArmAtHeightTask(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerMoveArmAtHeightTask>(() => moveAtHeightTask, ground);
        FarmerServiceLocator.MapService<FarmerMoveArmAtHeightTask>(() => moveAtHeightTask, ground);

        var provideWaterTask = new FarmerProvideWaterTask(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerProvideWaterTask>(() => provideWaterTask, ground);
        FarmerServiceLocator.MapService<FarmerProvideWaterTask>(() => provideWaterTask, ground);

        await moveOnGridTask.Initialize(cancellationToken);
        await moveAtHeightTask.Initialize(cancellationToken);
        await alertHandler.InitializeAsync(cancellationToken);
    }
}
