
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Movement;

public interface IFarmerMoveOnGridDevice
{
    Task<bool> MoveOnGridAsync(double x, double y, CancellationToken token);
}
