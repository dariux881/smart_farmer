using System.Threading.Tasks;
using SmartFarmer.Alerts;

namespace SmartFarmer.Utils;

public interface IFarmerAlertHandler : IFarmerServiceProvider<IFarmerAlert>
{
    Task<string> RaiseAlert(
        string message, 
        AlertCode code, 
        string taskId, 
        string plantInstanceId, 
        string groundId,
        AlertLevel level, 
        AlertSeverity severity);
    
    Task<string> RaiseAlert(FarmerAlertRequestData data);

    Task<IFarmerAlert> GetAlertById(string alertId);
}