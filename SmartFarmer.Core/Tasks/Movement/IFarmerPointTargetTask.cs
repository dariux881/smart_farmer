using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement;

public interface IFarmerPointTargetTask : IFarmerTask, IHasTargetDegrees
{
    Task<object> TurnDeviceToDegrees(double degrees, CancellationToken token);
    double GetCurrentDegrees();
}