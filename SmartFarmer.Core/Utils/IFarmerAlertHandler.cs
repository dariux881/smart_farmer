using SmartFarmer.Alerts;

namespace SmartFarmer.Utils;

public interface IFarmerAlertHandler
{
    void RaiseAlert(
        string message, 
        AlertCode code, 
        string taskId, 
        string plantInstanceId, 
        AlertLevel level, 
        AlertSeverity severity);
}