
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;

namespace SmartFarmer.Movement;

public interface IFarmerMoveOnGridDevice : IFarmer2dPointNotifier
{
    Task<bool> MoveOnGridAsync(double x, double y, CancellationToken token);
}
