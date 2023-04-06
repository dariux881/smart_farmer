using System;
using SmartFarmer.Alerts;

namespace SmartFarmer.Exceptions;

public class FarmerTaskExecutionException : FarmerBaseException
{
    public string TaskName { get; protected set; }

    public FarmerTaskExecutionException(
        string taskId,
        string plantInstanceId,
        string message = null, 
        Exception innerException = null,
        AlertCode? code = null,
        AlertLevel? level = null,
        AlertSeverity? severity = null)
        : base(message, innerException)
    {
        RaisedByTaskId = taskId;
        PlantId = plantInstanceId;

        Code = code ?? Alerts.AlertCode.InvalidProgramConfiguration;
        Level = level ?? Alerts.AlertLevel.Error;
        Severity = severity ?? Alerts.AlertSeverity.High;
    }
}