using System.Threading.Tasks;
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

        var groundManager = new GroundActivityManager()
        {
            OperationalMode = OperationalManagement.AppOperationalMode.Console
        };
        await groundManager.Run();
    }

    private static void InitializeServices()
    {
        FarmerServiceLocator.InitializeServiceLocator();

        FarmerServiceLocator.MapService<IFarmerAlertHandler>(() => new FarmerAlertHandler());
        FarmerServiceLocator.MapService<IFarmerTaskProvider>(() => new FarmerTaskProvider());
    }
}
