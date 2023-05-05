
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Movement;

public interface IFarmerMoveAtHeightDevice
{
    Task<bool> MoveArmAtHeightAsync(double heightInCm, CancellationToken token);
    Task<int> MoveArmAtMaxHeightAsync(CancellationToken token);
}