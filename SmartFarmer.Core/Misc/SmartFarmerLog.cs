using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace SmartFarmer.Misc
{
    public static class SmartFarmerLog 
    {
        static SmartFarmerLog()
        {
            // Log.Logger = new LoggerConfiguration()
            //     .MinimumLevel.Debug()
            //     //.WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
            //     .WriteTo.Console()
            //     .CreateLogger();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Information("logger created");
        }

        public static void Information(string message)
        {
            Log.Information(message);
        }

        public static void Debug(string message)
        {
            Log.Debug(message);
        }

        public static void Warning(string message)
        {
            Log.Warning(message);
        }

        public static void Error(string message)
        {
            Log.Error(message);
        }

        public static void Exception(Exception ex)
        {
            Log.Error("[EXC] " + ex?.Message + "\n" + ex?.StackTrace);
        }
    }
}