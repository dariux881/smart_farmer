
using System;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Utils;

namespace SmartFarmer.Alerts;

public class FarmerAlertHandler : IFarmerAlertHandler
{
    private static readonly Lazy<FarmerAlertHandler> _instance = new(() => new FarmerAlertHandler(FarmerAlertProvider.Instance));
    private IFarmerAlertProvider _alertProvider;

    public static FarmerAlertHandler Instance => _instance.Value;

    private FarmerAlertHandler(IFarmerAlertProvider alertProvider)
    {
        _alertProvider = alertProvider;
    }

    public event EventHandler<FarmerAlertHandlerEventArgs> NewAlertCreated;

    public void RaiseAlert(string message, string code, string taskId, string plantInstanceId, AlertLevel level, AlertSeverity severity)
    {
        var alert = new FarmerAlert
            {
                ID = FarmerAlertProvider.Instance.GenerateServiceId(),
                Message = message,
                Code = code,
                Level = level,
                Severity = severity
            };

        _alertProvider.AddFarmerService(alert);

        NewAlertCreated?.Invoke(this, new FarmerAlertHandlerEventArgs(alert.ID));
    }
    
    public void RaiseAlert(
            IFarmerTask task,
            IFarmerPlantInstance plant,
            string code,
            string message,
            AlertLevel level,
            AlertSeverity severity
        )
    {
        var now = DateTime.UtcNow;

        var alert =  
            new FarmerAlert()
                {
                    ID = _alertProvider.GenerateServiceId(),
                    RaisedByTaskId = task?.ID,
                    When = now,
                    PlantInstanceId = plant?.ID,
                    Code = code,
                    Message = message,
                    Level = level,
                    Severity = severity,
                    MarkedAsRead = false
                };

        NewAlertCreated?.Invoke(
            this, 
            new FarmerAlertHandlerEventArgs(
                alert.ID
            ));
    }
}