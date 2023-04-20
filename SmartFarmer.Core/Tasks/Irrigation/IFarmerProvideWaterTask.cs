using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Irrigation
{
    public interface IFarmerProvideWaterTask : IFarmerTask
    {
        Task ProvideWater(double amountInLiters, CancellationToken token);
    }
}
