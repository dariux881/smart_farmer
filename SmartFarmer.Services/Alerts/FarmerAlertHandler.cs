using System;
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
                ID = _alertProvider.GenerateServiceId(),
                Message = message,
                When = DateTime.UtcNow,
                Code = code,
                RaisedByTaskId = taskId,
                PlantInstanceId = plantInstanceId,
                Level = level,
                Severity = severity
            };

        var result = _alertProvider.AddFarmerService(alert);

        if (result)
        {
            NewAlertCreated?.Invoke(this, new FarmerAlertHandlerEventArgs(alert.ID));
        }
    }
}