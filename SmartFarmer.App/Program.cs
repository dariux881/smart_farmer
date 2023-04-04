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

        var groundManager = new GroundActivityManager();
        await groundManager.Run();
    }

    private static void InitializeServices()
    {
        FarmerServiceLocator.InitializeServiceLocator();

        FarmerServiceLocator.MapService<IFarmerAlertHandler>(() => new FarmerAlertHandler());
        FarmerServiceLocator.MapService<IFarmerTaskProvider>(() => new FarmerTaskProvider());
    }
}
