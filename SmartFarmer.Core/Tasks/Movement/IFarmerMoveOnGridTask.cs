using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement;

public interface IFarmerMoveOnGridTask : IFarmerTask, IHasTargetGridPosition
{
    Task<object> MoveToPosition(double x, double y, CancellationToken token);
    void GetCurrentPosition(out double x, out double y);
}