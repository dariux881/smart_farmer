using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Tasks.Generic;

public interface IRequiresInitialization
{
    Task<bool> InitializeAsync(CancellationToken token);
}