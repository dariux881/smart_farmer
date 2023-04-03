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
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer;

public class Program
{
    public static async Task Main(string[] args)
    {
        InitializeServices();

        //TODO move in Ground Manager
        await PrepareEnvironment();
        await InitializeGrounds();

        var plans = GetPlansToRun();
        await ExecutePlans(plans);
    }

    private static async Task ExecutePlans(IDictionary<string, IEnumerable<IFarmerPlan>> plans)
    {
        var tasks = new List<Task>();
        var cancellationToken = new CancellationToken();

        foreach (var groundPlans in plans)
        {
            var localGroundPlans = groundPlans.Value;
            tasks.Add(Task.Run(async () => {
                foreach(var plan in localGroundPlans)
                {
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
                    
                }
            }));
        }

        try {  
            await Task.WhenAll(tasks);  
        } 
        catch {}
    }

    private static IDictionary<string, IEnumerable<IFarmerPlan>> GetPlansToRun()
    {
        var plans = new Dictionary<string, IEnumerable<IFarmerPlan>>();

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
                        .ToList();
            
            if (plansInGround.Any())
            {
                plans.Add(ground.ID, plansInGround);
            }
        }

        return plans;
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
                    .Select(x => x.ID)
                    .Where(x => 
                        LocalConfiguration.LocalGroundIds.Contains(x)).ToList() :
                new List<string>() { grounds.First().ID };

        var tasks = new List<Task>();

        foreach (var value in locallyInterestedGrounds)
        {
            var groundId = value;
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
    
    private static void InitializeServices()
    {
        FarmerServiceLocator.InitializeServiceLocator();

        FarmerServiceLocator.MapService<IFarmerAlertHandler>(() => new FarmerAlertHandler());
        FarmerServiceLocator.MapService<IFarmerTaskProvider>(() => new FarmerTaskProvider());
        FarmerServiceLocator.MapService<IFarmerToolsManager>(() => new FarmerToolsManager());

        InitializeServicesForTasks();
    }

    private static void InitializeServicesForTasks()
    {
        var moveOnGrid = new FarmerMoveOnGridTask(null, null);
        FarmerServiceLocator.MapService<IFarmerMoveOnGridTask>(() => moveOnGrid);
        FarmerServiceLocator.MapService<FarmerMoveOnGridTask>(() => moveOnGrid);

        var moveAtHeight = new FarmerMoveArmAtHeight(null);
        FarmerServiceLocator.MapService<IFarmerMoveArmAtHeight>(() => moveAtHeight);
        FarmerServiceLocator.MapService<FarmerMoveArmAtHeight>(() => moveAtHeight);
    }
}
