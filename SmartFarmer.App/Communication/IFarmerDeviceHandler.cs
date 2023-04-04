
using System.Threading;
using System.Threading.Tasks;

public interface IFarmerMoveOnGridDevice : IFarmerPointNotifier
{
    Task<bool> MoveOnGridAsync(FarmerPoint position, CancellationToken token);
}

public interface IFarmerMoveAtHeightDevice : IFarmerPointNotifier
{
    Task<bool> MoveArmAtHeightAsync(double heightInCm, CancellationToken token);
    Task<bool> MoveArmAtMaxHeightAsync(CancellationToken token);
}

public interface IFarmerTurnToolDevice : IFarmerPointNotifier
{
    Task<bool> TurnArmToDegreesAsync(double degrees, CancellationToken token);
    Task<bool> PointDeviceAsync(double degrees, CancellationToken token);
}
