using System.Threading;
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
        string gardenId,
        AlertLevel level, 
        AlertSeverity severity);
    
    Task<string> RaiseAlert(FarmerAlertRequestData data);

    Task<bool> MarkAlertAsReadAsync(string alertId, bool status, CancellationToken token);

    Task<IFarmerAlert> GetAlertById(string alertId);
}