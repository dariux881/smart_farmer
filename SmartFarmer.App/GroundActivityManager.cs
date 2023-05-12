using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SmartFarmer.Data;
using SmartFarmer.Handlers;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.OperationalManagement;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer;

public class GroundActivityManager
{
    private List<IOperationalModeManager> _operationalManagers;
    private ApiConfiguration _apiConfiguration;
    private UserConfiguration _userConfiguration;
    private AppConfiguration _appConfiguration;
    private SerialCommunicationConfiguration _serialConfiguration;
    private HubConnectionConfiguration _hubConfiguration;
    private Dictionary<string, FarmerGroundHubHandler> _hubHandlers;

    public async Task Run()
    {
        _hubHandlers = new Dictionary<string, FarmerGroundHubHandler>();

        PrepareEnvironment();

        FillOperationalManagers();

        var loginResult = await Login();
        if (!loginResult)
        {
            //TODO retry in case of failure
            SmartFarmerLog.Error($"Login failed for user {_userConfiguration.UserName}. Stopping manager");
            return;
        }

        await InitializeGroundsAsync();
        await InitializeHubsForGroundsAsync();

        var tokenSource = new CancellationTokenSource();
        var cancellationToken = tokenSource.Token;

        var tasks = new List<Task>();
        foreach (var opManager in _operationalManagers)
        {
            tasks.Add(Task.Run(() => opManager.Run(cancellationToken)));
        }

        await Task.WhenAll(tasks);
    }

    private async Task InitializeHubsForGroundsAsync()
    {
        var closingHubs = _hubHandlers.Values.Where(x => x is IDisposable).Cast<IDisposable>();

        foreach (var hub in closingHubs)
        {
            hub.Dispose();
        }

        _hubHandlers.Clear();

        foreach(var ground in LocalConfiguration.Grounds)
        {
            var hub = new FarmerGroundHubHandler(ground.Value, _hubConfiguration);
            await hub.InitializeAsync();

            _hubHandlers.TryAdd(ground.Key, hub);
        }
    }

    private async Task<bool> Login()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var user = new Data.Security.LoginRequestData() {
                UserName = _userConfiguration?.UserName, 
                Password = _userConfiguration?.Password
            };

        // login
        var loginResponse = await FarmerRequestHelper.Login(
            user, 
            cancellationToken);

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

        if (_appConfiguration.AppOperationalMode == null)
        {
            return;
        }

        if (_appConfiguration.AppOperationalMode.Value.HasFlag(AppOperationalMode.Console))
        {
            _operationalManagers.Add(new ConsoleOperationalModeManager());
        }

        if (_appConfiguration.AppOperationalMode.Value.HasFlag(AppOperationalMode.Auto))
        {
            _operationalManagers.Add(new AutomaticOperationalManager(_appConfiguration));
        }

        if (_appConfiguration.AppOperationalMode.Value.HasFlag(AppOperationalMode.RemoteCLI))
        {
            _operationalManagers.Add(new RemoteCommandLineInterfaceOperationalManager(_hubConfiguration));
        }

        _operationalManagers.ForEach(async opMan =>
        {
            await opMan.Prepare();
            opMan.NewOperationRequired += ExecuteRequiredOperation;
        });

    }

    private void ExecuteRequiredOperation(object sender, OperationRequestEventArgs e)
    {
        SmartFarmerLog.Information($"Operation {e.Operation} requested by {e.Sender.Name}");

        switch(e.Operation)
        {
            case AppOperation.RunPlan:
                {
                    Task.Run(async () => await ExecutePlan(e.AdditionalData.FirstOrDefault()));
                }

                break;

            case AppOperation.UpdateAllGrounds:
                ClearLocalData();
                Task.Run(async () => 
                {
                    await InitializeGroundsAsync();
                    await InitializeHubsForGroundsAsync();
                });
                break;

            case AppOperation.MarkAlert:
                Task.Run(async () => await InvertAlertReadStatus(e.AdditionalData.FirstOrDefault()));
                break;
            
            case AppOperation.TestPosition:
                Task.Run(async () => await SendTestPosition(e.AdditionalData.FirstOrDefault()));
                break;

            default: 
                throw new NotSupportedException();
        }
    }

    private async Task SendTestPosition(string groundId)
    {
        await _hubHandlers[groundId].SendDevicePosition(new Movement.FarmerDevicePositionRequestData()
        {
            GroundId = groundId,
            PositionDt = DateTime.UtcNow,
            Position = new Farmer5dPoint(1, 2, 3, 4, 5)
        });
    }

    private async Task<bool> InvertAlertReadStatus(string alertId)
    {
        var ground = GroundUtils.GetGroundByAlert(alertId) as FarmerGround;
        if (ground == null) return false;

        var alert = ground.Alerts.First(x => x.ID == alertId);
        var newState = !alert.MarkedAsRead;
        
        return await MarkAlertAsRead(ground.ID, alertId, newState);
    }

    private static async Task ExecutePlan(string planId)
    {
        var ground = GroundUtils.GetGroundByPlan(planId);
        if (ground == null || ground is not FarmerGround fGround)
        {
            SmartFarmerLog.Error("No valid ground found for plan " + planId);
            return;
        }

        await fGround.ExecutePlan(planId, CancellationToken.None);
    }

    private static async Task ExecutePlans(string[] planIds)
    {
        var tasks = new List<Task>();
        var cancellationToken = new CancellationToken();

        foreach (var planId in planIds)
        {
            var ground = GroundUtils.GetGroundByPlan(planId) as FarmerGround;

            if (ground == null) continue;
            var plan = ground.Plans.First(x => x.ID == planId);

            tasks.Add(
                Task.Run(async () => {
                    try
                    {
                        await plan.Execute(cancellationToken);
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

    private async Task<bool> MarkAlertAsRead(string groundId, string alertId, bool status)
    {
        return await FarmerServiceLocator.GetService<IFarmerAlertHandler>(true, groundId).MarkAlertAsRead(alertId, status);
    }

    private async Task InitializeGroundsAsync()
    {
        var cancellationToken = new CancellationToken();

        // get all grounds
        var grounds = await FarmerRequestHelper.GetGroundsList(cancellationToken);
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
            LocalConfiguration.LocalGroundIds.Any() ?
                grounds
                    .Where(x => 
                        LocalConfiguration.LocalGroundIds.Contains(x.ID)).ToList() :
                new List<IFarmerGround>() { grounds.First() };

        var tasks = new List<Task>();

        foreach (var ground in locallyInterestedGrounds)
        {
            await InitializeServicesForSingleGround(ground, cancellationToken);

            var groundId = ground.ID;
            tasks.Add(Task.Run(async () => {
                var ground = await FarmerRequestHelper
                    .GetGround(groundId, cancellationToken);
                
                LocalConfiguration.Grounds.TryAdd(ground.ID, ground);
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

        var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true);

        IConfiguration config = builder.Build();

        _apiConfiguration = config.GetSection("ApiConfiguration").Get<ApiConfiguration>();
        _userConfiguration = config.GetSection("UserConfiguration").Get<UserConfiguration>();
        _appConfiguration = config.GetSection("AppConfiguration").Get<AppConfiguration>();
        _serialConfiguration = config.GetSection("SerialConfiguration").Get<SerialCommunicationConfiguration>();
        _hubConfiguration = config.GetSection("HubConnectionConfiguration").Get<HubConnectionConfiguration>();
    }
    
    private async Task InitializeServicesForSingleGround(IFarmerGround ground, CancellationToken cancellationToken)
    {
        // clearing possibly old mapped services
        FarmerServiceLocator.RemoveService<IFarmerToolsManager>();
        FarmerServiceLocator.RemoveService<IFarmerMoveOnGridTask>();
        FarmerServiceLocator.RemoveService<FarmerMoveOnGridTask>();
        FarmerServiceLocator.RemoveService<IFarmerMoveArmAtHeightTask>();
        FarmerServiceLocator.RemoveService<FarmerMoveArmAtHeightTask>();

        // preparing new services
        var alertHandler = new FarmerAlertHandler(ground, _hubConfiguration);

        FarmerServiceLocator.MapService<IFarmerToolsManager>(() => new FarmerToolsManager(ground));
        FarmerServiceLocator.MapService<IFarmerAlertHandler>(() => alertHandler, ground);

        var deviceHandler = new ExternalDeviceProxy(ground, _serialConfiguration, _hubConfiguration);

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
        await alertHandler.InitializeAsync();
    }
}
