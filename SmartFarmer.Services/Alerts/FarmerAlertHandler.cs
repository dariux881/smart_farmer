using System;
using System.Threading.Tasks;
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

    public async Task<bool> MarkAlertAsRead(string alertId, bool status)
    {
        var alert = await _alertProvider.GetFarmerService(alertId) as FarmerAlert;
        if (alert != null)
        {
            alert.MarkedAsRead = status;
            return true;
        }

        return false;
    }

    public async Task<string> RaiseAlert(
        string message, 
        AlertCode code, 
        string taskId, 
        string plantInstanceId,
        string groundId,
        AlertLevel level, 
        AlertSeverity severity)
    {
        var alert = new FarmerAlert
            {
                Message = message,
                When = DateTime.UtcNow,
                Code = code,
                RaisedByTaskId = taskId,
                PlantInstanceId = plantInstanceId,                
                Level = level,
                Severity = severity
            };

        var result = await _alertProvider.AddFarmerService(alert);

        if (result != null)
        {
            NewAlertCreated?.Invoke(this, new FarmerAlertHandlerEventArgs(result));
        }

        return result;
    }

    public async Task<string> RaiseAlert(FarmerAlertRequestData data)
    {
        var alert = new FarmerAlert
            {
                Message = data.Message,
                When = DateTime.UtcNow,
                Code = data.Code,
                RaisedByTaskId = data.RaisedByTaskId,
                PlantInstanceId = data.PlantInstanceId,
                Level = data.Level,
                Severity = data.Severity
            };

        var result = await _alertProvider.AddFarmerService(alert);

        if (result != null)
        {
            NewAlertCreated?.Invoke(this, new FarmerAlertHandlerEventArgs(result));
        }

        return result;
    }


    public async Task<IFarmerAlert> GetAlertById(string alertId)
    {
        return await _alertProvider.GetFarmerService(alertId);
    }

    public async Task<string> AddFarmerService(IFarmerAlert service)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<IFarmerAlert> GetFarmerService(string serviceId)
    {
        return await GetAlertById(serviceId);
    }
}