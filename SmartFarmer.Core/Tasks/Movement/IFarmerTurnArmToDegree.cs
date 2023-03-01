using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement;

public interface IFarmerTurnArmToDegree : IFarmerTask
{
    Task TurnArmToDegree(double degree, CancellationToken token);
}