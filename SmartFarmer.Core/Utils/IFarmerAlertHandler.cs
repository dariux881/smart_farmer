using SmartFarmer.Alerts;

namespace SmartFarmer.Utils;

public interface IFarmerAlertHandler
{
    void RaiseAlert(
        string message, 
        string code, 
        string taskId, 
        string plantInstanceId, 
        AlertLevel level, 
        AlertSeverity severity);
}