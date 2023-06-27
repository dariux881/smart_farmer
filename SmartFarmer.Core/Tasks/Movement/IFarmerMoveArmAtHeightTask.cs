using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement;

public interface IFarmerMoveArmAtHeightTask : IFarmerTask, IHasTargetHeight
{   
    Task<object> MoveToHeight(double heightInCm, CancellationToken token);
    double GetCurrentHeight();
}