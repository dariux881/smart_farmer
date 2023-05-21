using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.OperationalManagement;

public interface IOperationalModeManager : IDisposable
{
    AppOperationalMode Mode { get; }
    string Name { get; }
    event EventHandler<OperationRequestEventArgs> NewOperationRequired;
    Task InitializeAsync(CancellationToken token);
    Task Run(CancellationToken token);
    void ProcessResult(OperationRequestEventArgs args);
}
