using System;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Configurations;
using SmartFarmer.Data;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Utils;

namespace SmartFarmer.Handlers;

public class FarmerAlertHandler : IFarmerAlertHandler
{
    private IFarmerGround _ground;
    private FarmerGroundHubHandler _hubHandler;

    public FarmerAlertHandler(IFarmerGround ground, HubConnectionConfiguration hubConfiguration)
    {
        _ground = ground;

        _hubHandler = new FarmerGroundHubHandler(ground, hubConfiguration);

        _hubHandler.NewAlertStatusEventArgsReceived += AlertStatusChanged;
    }

    private void AlertStatusChanged(object sender, NewAlertStatusEventArgs e)
    {
        SmartFarmerLog.Debug($"Alert {e.AlertId} changed in {e.Status}");
        LocallyUpdateAlert(e.AlertId, e.Status);
    }

    public async Task InitializeAsync()
    {
        await _hubHandler.InitializeAsync();
    }

    public async Task<string> AddFarmerService(IFarmerAlert service)
    {
        return await RaiseAlert(
            service.Message,
            service.Code,
            service.RaisedByTaskId,
            service.PlantInstanceId,
            _ground.ID,
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

    public async Task<bool> MarkAlertAsRead(string alertId, bool status)
    {
        var result = await FarmerRequestHelper.MarkAlertAsRead(alertId, status, System.Threading.CancellationToken.None);

        if (result)
        {
            LocallyUpdateAlert(alertId, status);
            await _hubHandler.NotifyNewAlertStatus(alertId, status);
        }

        return result;
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
        if (groundId != _ground.ID)
        {
            throw new InvalidOperationException($"this instance handles only alerts for ground {_ground.ID}");
        }

        return await RaiseAlert(
            new FarmerAlertRequestData()
            {
                Message = message,
                Code = code,
                RaisedByTaskId = taskId,
                PlantInstanceId = plantInstanceId,
                Level = level,
                Severity = severity,
                FarmerGroundId = groundId
            });
    }

    public async Task<string> RaiseAlert(FarmerAlertRequestData data)
    {
        var alertId = await FarmerRequestHelper.RaiseAlert(data, System.Threading.CancellationToken.None).ConfigureAwait(false);
        var alert = await GetAlertById(alertId);

        if (alert == null) return null;

        LocalConfiguration.Grounds.TryGetValue(data.FarmerGroundId, out var ground);
        if (ground != null && ground is FarmerGround fGround && alert != null)
        {
            SmartFarmerLog.Debug("adding alert to ground " + ground.ID);
            fGround.AddAlert(alert);
        }
        else
        {
            SmartFarmerLog.Warning("ground found? " + (ground != null) + " alert found? " + (alert != null));
        }

        return alert.ID;
    }

    private void LocallyUpdateAlert(string alertId, bool status)
    {
        var ground = GroundUtils.GetGroundByAlert(alertId) as FarmerGround;
        if (ground == null || ground.ID != _ground.ID) 
        {
            return;
        }

        var alert = ground.Alerts.FirstOrDefault(x => x.ID == alertId);
        if (alert != null)
        {
            alert.MarkedAsRead = status;
        }
    }
}