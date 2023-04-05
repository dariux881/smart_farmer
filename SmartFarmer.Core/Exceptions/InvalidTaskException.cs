using System;

namespace SmartFarmer.Exceptions;

public class InvalidTaskException : FarmerBaseException
{
    public string TaskName { get; protected set; }

    public InvalidTaskException(
        string message = null, 
        Exception innerException = null,
        string taskName = null)
        : base(message, innerException)
    {
        TaskName = taskName;

        Code = Alerts.AlertCode.InvalidProgramConfiguration;
        Level = Alerts.AlertLevel.Error;
        Severity = Alerts.AlertSeverity.High;
    }
}
