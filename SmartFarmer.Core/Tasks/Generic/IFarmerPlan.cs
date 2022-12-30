using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic
{

    public interface IFarmerPlan : IHasProgressCheckInfo
    {
        string Name { get; }
        IReadOnlyList<IFarmerPlanStep> Steps { get; }
        Task Execute(CancellationToken token);
    }
}
