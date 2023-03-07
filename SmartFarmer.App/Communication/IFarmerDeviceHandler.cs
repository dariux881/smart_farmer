
using System.Threading;
using System.Threading.Tasks;

public interface IFarmerDeviceHandler : IFarmerPointNotifier
{
    Task<bool> MoveOnGridAsync(FarmerPoint position, CancellationToken token);
    Task<bool> MoveArmAtheightAsync(int heightInCm, CancellationToken token);
    Task<bool> TurnArmToDegreesAsync(double degrees, CancellationToken token);
    Task<bool> PointDeviceAsync(double degrees, CancellationToken token);
}