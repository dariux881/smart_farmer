using System;
using SmartFarmer.Tasks;

namespace SmartFarmer.OperationalManagement;

public class OperationRequestEventArgs : EventArgs
{
    public OperationRequestEventArgs(
        IOperationalModeManager sender, 
        AppOperation operation,
        string[] additionalData)
    {
        Sender = sender;
        Operation = operation;
        AdditionalData = additionalData;
    }

    public IOperationalModeManager Sender { get; }
    public AppOperation Operation { get; }
    public string[] AdditionalData { get; }
    public FarmerPlanExecutionResult Result { get; set; }
    public bool IsError { get; set; }
}
