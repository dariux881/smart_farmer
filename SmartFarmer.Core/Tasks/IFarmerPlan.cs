using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer
{

    public interface IFarmerPlan
    {
        bool IsPlanInProgress { get; }
        IList<IFarmerPlanStep> Steps { get; }
        Task Execute(CancellationToken token);
    }
}
