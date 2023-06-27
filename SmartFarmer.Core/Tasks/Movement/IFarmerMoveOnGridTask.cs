using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement;

public interface IFarmerMoveOnGridTask : IFarmerTask
{
    double TargetXInCm { get; }
    double TargetYInCm { get; }

    Task<object> MoveToPosition(double x, double y, CancellationToken token);
    void GetCurrentPosition(out double x, out double y);
}