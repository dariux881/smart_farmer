
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Movement;

public interface IFarmerWaterProviderDevice
{
    Task<double> GetCurrentHumidityLevel(CancellationToken token);
    Task<bool> ProvideWaterAsync(int pumpNumber, double amountInLiters, CancellationToken token);
}