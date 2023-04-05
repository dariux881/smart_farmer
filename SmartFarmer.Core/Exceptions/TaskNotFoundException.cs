using System;

namespace SmartFarmer.Exceptions;

public class TaskNotFoundException : FarmerBaseException
{
    public string TaskName { get; protected set; }

    public TaskNotFoundException(
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
