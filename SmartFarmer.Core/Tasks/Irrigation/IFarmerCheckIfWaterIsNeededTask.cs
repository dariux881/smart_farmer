using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Irrigation;

public interface IFarmerCheckIfWaterIsNeededTask : IFarmerTask
{
    double ExpectedAmountInLiters { get; }
    Task<bool> IsWaterNeeded(double expectedAmountInLiters, CancellationToken token);
}