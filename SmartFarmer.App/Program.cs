using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SmartFarmer.Handlers;
using SmartFarmer.Handlers.Providers;
using SmartFarmer.Misc;
using SmartFarmer.Utils;

namespace SmartFarmer;

public class Program
{
    public static async Task Main(string[] args)
    {
        InitializeServices();

        var groundManager = new GroundActivityManager();
        await groundManager.Run();
    }

    private static void InitializeServices()
    {
        FarmerServiceLocator.InitializeServiceLocator();

        FarmerServiceLocator.MapService<IFarmerTaskProvider>(() => new FarmerTaskProvider());
        FarmerServiceLocator.MapService<IFarmerAppCommunicationHandler>(() => new FarmerAppCommunicationHandler());

        FarmerServiceLocator.MapService<IFarmerDeviceKindFactory>(() => new FarmerDeviceKindFactory());
        FarmerServiceLocator.MapService<IFarmerDeviceKindProvider>(() => new FarmerDeviceKindProvider());

        var builder = 
            new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true);

        IConfiguration config = builder.Build();

        var configProvider = new AppsettingsBasedConfigurationProvider(config);

        FarmerServiceLocator.MapService<IFarmerConfigurationProvider>(() => configProvider);
        FarmerServiceLocator.MapService<IFarmerLocalInformationManager>(() => new FarmerLocalInformationManager());
    }
}
