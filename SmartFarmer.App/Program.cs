using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using SmartFarmer.Misc;
using SmartFarmer.Utils;

namespace SmartFarmer;

public class Program
{
    static void Main(string[] args)
    {
        SmartFarmerLog.SetAlertProvider(FarmerAlertProvider.Instance);

        Console.WriteLine("Hello World!");
    }
}
