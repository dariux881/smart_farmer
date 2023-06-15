using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Irrigation;

public interface IFarmerProvideWaterTask : IFarmerTask
{
    int PumpNumber { get; }
    double WaterAmountInLiters { get; }
    
    Task ProvideWater(int pumpNumber, double amountInLiters, CancellationToken token);
}