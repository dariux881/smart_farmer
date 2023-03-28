using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmartFarmer.Alerts;
using SmartFarmer.Utils;

namespace SmartFarmer.Misc;

public static class SmartFarmerLog 
{
    private static IFarmerAlertHandler _alertHandler;

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

    public static void SetAlertHandler(IFarmerAlertHandler alertHandler)
    {
        _alertHandler = alertHandler;
    }

    public static void Information(string message)
    {
        Log.Information(message);
    }

    public static void Debug(string message)
    {
        Log.Debug(message);
    }

    public static void Warning(string message, FarmerAlertRequestData alert = null)
    {
        Log.Warning(message);

        if (alert != null && _alertHandler != null)
        {
            Task.Run(async () => await _alertHandler.RaiseAlert(alert));
        }
    }

    public static void Error(string message, FarmerAlertRequestData alert = null)
    {
        Log.Error(message);

        if (alert != null && _alertHandler != null)
        {
            Task.Run(async () => await _alertHandler.RaiseAlert(alert));
        }
    }

    public static void Exception(Exception ex, FarmerAlertRequestData alert = null)
    {
        var innerMessage = ex?.InnerException != null ? 
            "\n" + ex?.InnerException.Message : 
            string.Empty;

        Log.Error("[EXC] " + ex?.Message + 
                    innerMessage +
                    "\n" + ex?.StackTrace);

        if (alert != null && _alertHandler != null)
        {
           Task.Run(async () => await _alertHandler.RaiseAlert(alert));
        }
    }
}