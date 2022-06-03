using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks
{

    public interface IFarmerPlan : IHasProgressCheckInfo
    {
        IList<IFarmerPlanStep> Steps { get; }
        Task Execute(CancellationToken token);
    }
}
