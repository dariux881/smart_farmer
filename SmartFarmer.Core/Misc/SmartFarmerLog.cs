using System;
using Serilog;

namespace SmartFarmer.Misc
{
    public static class SmartFarmerLog 
    {
        static SmartFarmerLog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                //.WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
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