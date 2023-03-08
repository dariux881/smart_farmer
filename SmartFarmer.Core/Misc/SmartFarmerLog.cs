using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmartFarmer.Alerts;
using SmartFarmer.Utils;

namespace SmartFarmer.Misc;

public static class SmartFarmerLog 
{
    private static IFarmerAlertProvider _alertProvider;

    static SmartFarmerLog()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
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

    public static void SetAlertProvider(IFarmerAlertProvider alertProvider)
    {
        _alertProvider = alertProvider;
    }

    public static void Information(string message)
    {
        Log.Information(message);
    }

    public static void Debug(string message)
    {
        Log.Debug(message);
    }

    public static void Warning(string message, IFarmerAlert alert = null)
    {
        Log.Warning(message);

        if (alert != null && _alertProvider != null)
        {
            _alertProvider.AddFarmerService(alert);
        }
    }

    public static void Error(string message, IFarmerAlert alert = null)
    {
        Log.Error(message);

        if (alert != null && _alertProvider != null)
        {
            _alertProvider.AddFarmerService(alert);
        }
    }

    public static void Exception(Exception ex, IFarmerAlert alert = null)
    {
        var innerMessage = ex?.InnerException != null ? 
            "\n" + ex?.InnerException.Message : 
            string.Empty;

        Log.Error("[EXC] " + ex?.Message + 
                    innerMessage +
                    "\n" + ex?.StackTrace);

        if (alert != null && _alertProvider != null)
        {
            _alertProvider.AddFarmerService(alert);
        }
    }
}