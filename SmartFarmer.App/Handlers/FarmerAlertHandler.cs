using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Configurations;
using SmartFarmer.Data;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Misc;
using SmartFarmer.Utils;

namespace SmartFarmer.Handlers;

public class FarmerAlertHandler : IFarmerAlertHandler
{
    private IFarmerGarden _garden;
    private FarmerGardenHubHandler _hubHandler;

    public FarmerAlertHandler(IFarmerGarden garden, HubConnectionConfiguration hubConfiguration)
    {
        _garden = garden;

        _hubHandler = new FarmerGardenHubHandler(garden, hubConfiguration);

        _hubHandler.NewAlertStatusEventArgsReceived += AlertStatusChanged;
    }

    private void AlertStatusChanged(object sender, NewAlertStatusEventArgs e)
    {
        SmartFarmerLog.Debug($"Alert {e.AlertId} changed in {e.Status}");
        LocallyUpdateAlert(e.AlertId, e.Status);
    }

    public async Task InitializeAsync(CancellationToken token)
    {
        await _hubHandler.InitializeAsync(token);
    }

    public async Task<string> AddFarmerService(IFarmerAlert service)
    {
        return await RaiseAlert(
            service.Message,
            service.Code,
            service.RaisedByTaskId,
            service.PlantInstanceId,
            _garden.ID,
            service.Level,
            service.Severity);
    }

    public async Task<IFarmerAlert> GetFarmerService(string serviceId)
    {
        return await GetAlertById(serviceId);
    }

    public async Task<IFarmerAlert> GetAlertById(string alertId)
    {
        return await FarmerRequestHandler.GetAlert(alertId, System.Threading.CancellationToken.None);
    }

    public async Task<bool> MarkAlertAsReadAsync(string alertId, bool status, CancellationToken token)
    {
        var result = await FarmerRequestHandler.MarkAlertAsRead(alertId, status, token);

        if (result)
        {
            LocallyUpdateAlert(alertId, status);
            await _hubHandler.NotifyNewAlertStatus(alertId, status, token);
        }

        return result;
    }

    public async Task<string> RaiseAlert(
        string message, 
        AlertCode code, 
        string taskId, 
        string plantInstanceId, 
        string gardenId,
        AlertLevel level, 
        AlertSeverity severity)
    {
        if (gardenId != _garden.ID)
        {
            throw new InvalidOperationException($"this instance handles only alerts for garden {_garden.ID}");
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
                GardenId = gardenId
            });
    }

    public async Task<string> RaiseAlert(FarmerAlertRequestData data)
    {
        var alertId = await FarmerRequestHandler.RaiseAlert(data, System.Threading.CancellationToken.None).ConfigureAwait(false);
        var alert = await GetAlertById(alertId);

        if (alert == null) return null;

        FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true).Gardens.TryGetValue(data.GardenId, out var garden);
        if (garden != null && garden is FarmerGarden fGarden && alert != null)
        {
            SmartFarmerLog.Debug("adding alert to garden " + garden.ID);
            fGarden.AddAlert(alert);
        }
        else
        {
            SmartFarmerLog.Warning("garden found? " + (garden != null) + " alert found? " + (alert != null));
        }

        return alert.ID;
    }

    private void LocallyUpdateAlert(string alertId, bool status)
    {
        var garden = GardenUtils.GetGardenByAlert(alertId) as FarmerGarden;
        if (garden == null || garden.ID != _garden.ID) 
        {
            return;
        }

        var alert = garden.Alerts.FirstOrDefault(x => x.ID == alertId);
        if (alert != null)
        {
            alert.MarkedAsRead = status;
        }
    }
}