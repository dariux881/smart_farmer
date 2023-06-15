using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic;

public interface IFarmerTask : IHasProgressCheckInfo, IFarmerService
{
    string TaskName { get; }
    FarmerTool RequiredTool { get; }
    Task Execute(CancellationToken token);
}
