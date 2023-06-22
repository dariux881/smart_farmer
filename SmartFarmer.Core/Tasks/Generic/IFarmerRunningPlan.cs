using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic;

public interface IFarmerRunningPlan : IFarmerPlan, IHasProgressCheckInfo
{
    Task Execute(CancellationToken token);
}