
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Movement;

public interface IFarmerTurnToolDevice
{
    Task<bool> TurnArmToDegreesAsync(double degrees, CancellationToken token);
    Task<bool> PointDeviceAsync(double degrees, CancellationToken token);
}
