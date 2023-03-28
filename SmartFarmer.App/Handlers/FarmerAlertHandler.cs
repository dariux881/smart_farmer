using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Helpers;
using SmartFarmer.Utils;

namespace SmartFarmer.Handlers;

public class FarmerAlertHandler : IFarmerAlertHandler
{
    public async Task<string> AddFarmerService(IFarmerAlert service)
    {
        return await RaiseAlert(
            service.Message,
            service.Code,
            service.RaisedByTaskId,
            service.PlantInstanceId,
            service.Level,
            service.Severity);
    }

    public async Task<IFarmerAlert> GetFarmerService(string serviceId)
    {
        return await GetAlertById(serviceId);
    }

    public async Task<IFarmerAlert> GetAlertById(string alertId)
    {
        return await FarmerRequestHelper.GetAlert(alertId, System.Threading.CancellationToken.None);
    }

    public async Task<string> RaiseAlert(string message, AlertCode code, string taskId, string plantInstanceId, AlertLevel level, AlertSeverity severity)
    {
        return await RaiseAlert(
            new FarmerAlertRequestData()
            {
                Message = message,
                Code = code,
                RaisedByTaskId = taskId,
                PlantInstanceId = plantInstanceId,
                Level = level,
                Severity = severity,
                FarmerGroundId = LocalConfiguration.LocalGroundId
            });
    }

    public async Task<string> RaiseAlert(FarmerAlertRequestData data)
    {
        return await FarmerRequestHelper.RaiseAlert(data, System.Threading.CancellationToken.None);
    }
}