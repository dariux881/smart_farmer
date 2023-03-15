using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Utils;

namespace SmartFarmer;

public class Program
{
    public static async Task Main(string[] args)
    {
        SmartFarmerLog.SetAlertProvider(FarmerAlertProvider.Instance);

        var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true);

        IConfiguration config = builder.Build();

        var apiConfiguration = config.GetSection("ApiConfiguration").Get<ApiConfiguration>();

        var httpReq = new HttpRequest();

        var result = await httpReq.GetAsync(SmartFarmerApiConstants.GET_GROUNDS, System.Threading.CancellationToken.None);

        if (result == null)
        {
            SmartFarmerLog.Error("Invalid response");
            return;
        }

        if (result.IsSuccessStatusCode)
        {
            SmartFarmerLog.Debug(await result.Content.ReadAsStringAsync());
            return;
        }

        SmartFarmerLog.Error(result.StatusCode.ToString());
    }
}
