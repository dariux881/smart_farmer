using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement;

public interface IFarmerMoveArmAtMaxHeightTask : IFarmerTask
{
    Task MoveToMaxHeight(CancellationToken token);
    double GetCurrentHeight();
}
