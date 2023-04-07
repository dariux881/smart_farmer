using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.OperationalManagement;

public interface IOperationalModeManager : IDisposable
{
    string Name { get; }
    event EventHandler<OperationRequestEventArgs> NewOperationRequired;
    Task Prepare();
    Task Run(CancellationToken token);
}
