using System;
using SmartFarmer.Alerts;

namespace SmartFarmer.Exceptions;

public abstract class FarmerBaseException : Exception
{
    public AlertSeverity Severity { get; protected set; }
    public AlertLevel Level { get; protected set; }
    public AlertCode Code { get; protected set; }
    public string RaisedByTaskId { get; protected set; }
    public string PlantId { get; protected set; }

    public FarmerBaseException(string message = null, Exception innerException = null)
        : base(message, innerException)
    {
        
    }
}