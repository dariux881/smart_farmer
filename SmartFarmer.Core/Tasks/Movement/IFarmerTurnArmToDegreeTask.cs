using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement;

public interface IFarmerTurnArmToDegreeTask : IFarmerTask, IHasTargetDegrees
{
    Task<object> TurnArmToDegrees(double degrees, CancellationToken token);
    double GetCurrentDegrees();
}