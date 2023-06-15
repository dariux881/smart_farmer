using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmartFarmer.Alerts;
using SmartFarmer.Utils;

namespace SmartFarmer.Misc;

public static class SmartFarmerLog 
{
    private static Object lockObj = new Object();
    private static bool _showThreadInfo = false;

    static SmartFarmerLog()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        SmartFarmerLog.InitLogger(configuration);
    }

    private static void InitLogger(IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        Log.Information("logger created");
    }

    public static void SetShowThreadInfo(bool show)
    {
        _showThreadInfo = show;
    }

    public static void Information(string message)
    {
        Log.Information(message);
        ShowThreadInformation("Task #" + Task.CurrentId.ToString());
    }

    public static void Debug(string message)
    {
        Log.Debug(message);
        ShowThreadInformation("Task #" + Task.CurrentId.ToString());
    }

    public static void Warning(string message)
    {
        Log.Warning(message);
        ShowThreadInformation("Task #" + Task.CurrentId.ToString());
    }

    public static async Task<string> Warning(string message, FarmerAlertRequestData alert)
    {
        Warning(message);

        if (alert != null)
        {
            var alertHandler = FarmerServiceLocator.GetService<IFarmerAlertHandler>(true, alert.GardenId);
            return await alertHandler.RaiseAlert(alert);
        }
        return null;
    }

    public static void Error(string message)
    {
        Log.Error(message);
    }

    public static async Task<string> Error(string message, FarmerAlertRequestData alert)
    {
        Error(message);
        ShowThreadInformation("Task #" + Task.CurrentId.ToString());
        
        if (alert != null)
        {
            var alertHandler = FarmerServiceLocator.GetService<IFarmerAlertHandler>(true, alert.GardenId);
            return await alertHandler.RaiseAlert(alert);
        }
        return null;
    }

    public static void Exception(Exception ex)
    {
        var innerMessage = ex?.InnerException != null ? 
            "\n" + ex?.InnerException.Message : 
            string.Empty;

        Log.Error("[EXC] " + ex?.Message + 
                    innerMessage +
                    "\n" + ex?.StackTrace);
    }

    public static async Task<string> Exception(Exception ex, FarmerAlertRequestData alert)
    {
        Exception(ex);
        ShowThreadInformation("Task #" + Task.CurrentId.ToString());

        if (alert != null)
        {
            var alertHandler = FarmerServiceLocator.GetService<IFarmerAlertHandler>(true, alert.GardenId);
            return await alertHandler.RaiseAlert(alert);
        }
        return null;
    }

    private static void ShowThreadInformation(String taskName)
    {
        if (!_showThreadInfo) return;

        String msg = null;
        Thread thread = Thread.CurrentThread;
        lock(lockObj) {
            msg = String.Format("{0} thread information\n", taskName) +
                String.Format("   Background: {0}\n", thread.IsBackground) +
                String.Format("   Thread Pool: {0}\n", thread.IsThreadPoolThread) +
                String.Format("   Thread ID: {0}\n", thread.ManagedThreadId);
        }
        Console.WriteLine(msg);
    }
}