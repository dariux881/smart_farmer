using System;
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

        SmartFarmerLog.SetAlertHandler(FarmerServiceLocator.GetService<IFarmerAlertHandler>(true));

        var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true);

        IConfiguration config = builder.Build();

        var apiConfiguration = config.GetSection("ApiConfiguration").Get<ApiConfiguration>();

        var user = new Data.Security.LoginRequestData() {
                UserName = "test", 
                Password = "test"
            };

        // login
        var loginResponse = await FarmerRequestHelper.Login(
            user, 
            CancellationToken.None);

        // save login result
        if (loginResponse == null || !loginResponse.IsSuccess)
        {
            SmartFarmerLog.Error("Invalid login for user " + user.UserName + " error: " + loginResponse?.ErrorMessage);
            return;
        }

        LocalConfiguration.LoggedUserId = loginResponse.UserId;
        LocalConfiguration.Token = loginResponse.Token;

        // get grounds
        var grounds = await FarmerRequestHelper.GetGroundsList(CancellationToken.None);
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

        SmartFarmerLog.Debug("List of grounds:\n\t" + grounds.Select(x => x.ID).Aggregate((g1, g2) => g1 + ", " + g2));

        var ground = await FarmerRequestHelper.GetGround(grounds.First().ID, CancellationToken.None);
    }

    private static void InitializeServices()
    {
        FarmerServiceLocator.InitializeServiceLocator();

        FarmerServiceLocator.MapService<IFarmerAlertHandler>(() => new FarmerAlertHandler());
    }
}
