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
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer;

public class GroundActivityManager
{
    public async Task Run()
    {
        await PrepareEnvironment();
        await InitializeGrounds();

        var plans = GetPlansToRun();
        await ExecutePlans(plans);

        var menu = 0;
        do
        {
            menu = Prompt(out var additionalData);

            switch(menu)
            {
                case -1 : 
                    break;
                case 0 : 
                    foreach (var plan in LocalConfiguration.Grounds.First().Value.PlanIds)
                    {
                        Console.WriteLine(plan);
                    }

                    break;
                case 1 :
                    
                    var ground = 
                        LocalConfiguration
                            .Grounds
                                .First()
                                .Value as FarmerGround;
                    
                    var result = await ground.ExecutePlan(additionalData, CancellationToken.None);

                    break;
                case 2 : 
                    ClearLocalData();
                    await InitializeGrounds();
                    break;
                
                default:
                    break;
            }

        } while (menu >= 0);
    }

    private void ClearLocalData()
    {
        LocalConfiguration.Grounds = new Dictionary<string, IFarmerGround>();
    }

    private int Prompt(out string additionalData) 
    {
        string message = 
            "\n"+
            "0 - list plans\n" +
            "1 - execute plan\n" +
            "2 - update grounds\n"+
            "-1 - exit\n"+
            " select: ";

        Console.WriteLine(message);

        int choice;
        while (!int.TryParse(Console.ReadLine().Trim(), out choice))
        {
            choice = -1;
            Console.WriteLine("retry: \n select: ");
        }

        if (choice == 1)
        {
            Console.WriteLine("insert additional data: ");
            additionalData = Console.ReadLine().Trim();
            return choice;
        }

        additionalData = null;
        return choice;
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
