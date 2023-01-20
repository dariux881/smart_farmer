using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic
{

    public interface IFarmerPlan : IHasProgressCheckInfo, IFarmerService
    {
        string Name { get; }
        IReadOnlyList<string> StepIds { get; }
        Task Execute(CancellationToken token);
    }
}
