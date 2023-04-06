using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SmartFarmer.Data;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.OperationalManagement;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer;

public class GroundActivityManager
{
    private List<IOperationalModeManager> _operationalManagers;

    public AppOperationalMode OperationalMode { get; set; }

    public async Task Run()
    {
        FillOperationalManagers();

        await PrepareEnvironment();
        await InitializeGrounds();

        //TODO move in auto ground manager
        // var plans = GetPlansToRun();
        // await ExecutePlans(plans);

        var tasks = new List<Task>();
        foreach (var opManager in _operationalManagers)
        {
            tasks.Add(Task.Run(() => opManager.Run()));

        }

        await Task.WhenAll(tasks);
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

        if (OperationalMode.HasFlag(AppOperationalMode.Console))
        {
            _operationalManagers.Add(new ConsoleOperationalModeManager());
        }

        if (OperationalMode.HasFlag(AppOperationalMode.Auto))
        {
            //TODO
        }

        _operationalManagers.ForEach(opMan =>
        {
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
                Task.Run(async () => await InitializeGrounds());

                break;            
            case AppOperation.MarkAlert:
                Task.Run(async () => await InvertAlertReadStatus(e.AdditionalData.FirstOrDefault()));

                break;                
        }
    }

    private static IFarmerGround GetGroundByPlan(string planId)
    {
        return LocalConfiguration.Grounds.Values.FirstOrDefault(x => x.PlanIds.Contains(planId));
    }

    private static IFarmerGround GetGroundByAlert(string alertId)
    {
        return LocalConfiguration.Grounds.Values.FirstOrDefault(x => x.AlertIds.Contains(alertId));
    }

    private static async Task<bool> InvertAlertReadStatus(string alertId)
    {
        var ground = GetGroundByAlert(alertId) as FarmerGround;
        if (ground == null) return false;

        var alert = ground.Alerts.First(x => x.ID == alertId);
        var newState = !alert.MarkedAsRead;
        
        return await MarkAlertAsRead(alertId, newState);
    }

    private static async Task ExecutePlan(string planId)
    {
        var ground = GetGroundByPlan(planId);
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
            var ground = GetGroundByPlan(planId) as FarmerGround;

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

    private static IEnumerable<string> GetPlansToRun()
    {
        var plans = new List<string>();

        foreach (var gGround in LocalConfiguration.Grounds.Values)
        {
            var ground = gGround as FarmerGround;
            if (ground == null) continue;
            
            var now = DateTime.UtcNow;
            var today = now.DayOfWeek;

            var plansInGround = 
                ground
                    .Plans
                        .Where(x => 
                            (x.ValidFromDt == null || x.ValidFromDt <= now) && // valid start
                            (x.ValidToDt == null || x.ValidToDt > now)) // valid end
                        .Where(x => x.PlannedDays == null || !x.PlannedDays.Any() || x.PlannedDays.Contains(today)) // valid day of the week
                        .OrderBy(x => x.Priority)
                        .Select(x => x.ID)
                        .ToList();
            
            plans.AddRange(plansInGround);
        }

        return plans;
    }

    private void ClearLocalData()
    {
        LocalConfiguration.Grounds = new Dictionary<string, IFarmerGround>();
    }

    private static async Task<bool> MarkAlertAsRead(string alertId, bool status)
    {
        return await FarmerServiceLocator.GetService<IFarmerAlertHandler>(true).MarkAlertAsRead(alertId, status);
    }

    private static async Task InitializeGrounds()
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

        foreach (var value in locallyInterestedGrounds)
        {
            await InitializeServicesForTasks(value, cancellationToken);

            var groundId = value.ID;
            tasks.Add(Task.Run(async () => {
                var ground = await FarmerRequestHelper
                    .GetGround(groundId, cancellationToken);
                
                LocalConfiguration.Grounds.TryAdd(ground.ID, ground);
            }));
        }

        try {  
           await Task.WhenAll(tasks);
        }  
        catch {}

    }

    private static async Task PrepareEnvironment()
    {
        SmartFarmerLog.SetShowThreadInfo(true);
        SmartFarmerLog.SetAlertHandler(FarmerServiceLocator.GetService<IFarmerAlertHandler>(true));

        var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true);

        IConfiguration config = builder.Build();

        var apiConfiguration = config.GetSection("ApiConfiguration").Get<ApiConfiguration>();
        var userConfiguration = config.GetSection("UserConfiguration").Get<UserConfiguration>();

        var cancellationToken = new CancellationToken();

        var user = new Data.Security.LoginRequestData() {
                UserName = userConfiguration?.UserName, 
                Password = userConfiguration?.Password
            };

        // login
        var loginResponse = await FarmerRequestHelper.Login(
            user, 
            cancellationToken);

        // save login result
        if (loginResponse == null || !loginResponse.IsSuccess)
        {
            SmartFarmerLog.Error("Invalid login for user " + user.UserName + " error: " + loginResponse?.ErrorMessage);
            return;
        }

        LocalConfiguration.LoggedUserId = loginResponse.UserId;
        LocalConfiguration.Token = loginResponse.Token;
    }
    
    private static async Task InitializeServicesForTasks(IFarmerGround ground, CancellationToken cancellationToken)
    {
        FarmerServiceLocator.MapService<IFarmerToolsManager>(() => new FarmerToolsManager(ground));

        var deviceHandler = new ExternalDeviceProxy();

        var moveOnGrid = new FarmerMoveOnGridTask(ground, deviceHandler);
        FarmerServiceLocator.MapService<IFarmerMoveOnGridTask>(() => moveOnGrid, ground);
        FarmerServiceLocator.MapService<FarmerMoveOnGridTask>(() => moveOnGrid, ground);

        var moveAtHeight = new FarmerMoveArmAtHeight(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerMoveArmAtHeight>(() => moveAtHeight, ground);
        FarmerServiceLocator.MapService<FarmerMoveArmAtHeight>(() => moveAtHeight, ground);

        await moveOnGrid.Initialize(cancellationToken);
        await moveAtHeight.Initialize(cancellationToken);
    }
}
