using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement;

public interface IFarmerMoveArmAtHeightTask : IFarmerTask
{
    double TargetHeightInCm { get; }
    Task<object> MoveToHeight(double heightInCm, CancellationToken token);
    double GetCurrentHeight();
}