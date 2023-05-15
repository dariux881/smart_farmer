using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;

namespace SmartFarmer.OperationalManagement;

public abstract class OperationalModeManagerBase : IOperationalModeManager
{
    public abstract AppOperationalMode Mode { get; }
    public abstract string Name { get; }

    public event EventHandler<OperationRequestEventArgs> NewOperationRequired;

    public abstract void Dispose();
    public abstract Task InitializeAsync(CancellationToken token);
    public abstract void ProcessResult(OperationRequestEventArgs args);
    public abstract Task Run(CancellationToken token);

    protected void SendNewOperation(AppOperation operation, string[] data)
    {
        var args = new OperationRequestEventArgs(this, operation, data);

        try
        {
            NewOperationRequired?.Invoke(this, args);
        }
        catch (AggregateException ex)
        {
            SmartFarmerLog.Exception(ex);
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
        }
    }

}
