using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SmartFarmer.Handlers;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
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

        Task t = Task.WhenAll(tasks);  
        try {  
            t.Wait();  
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
    }
}
