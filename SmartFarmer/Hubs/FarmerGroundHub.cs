using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartFarmer.Controllers;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Services;

namespace SmartFarmer.Hubs;

public class FarmerGroundHub : Hub, IDisposable
{
    private readonly ILogger<FarmerGroundController> _logger;
    private readonly ISmartFarmerGroundControllerService _groundProvider;
    private readonly ISmartFarmerUserAuthenticationService _userManager;

    public FarmerGroundHub(
        ILogger<FarmerGroundController> logger,
        ISmartFarmerGroundControllerService groundProvider,
        ISmartFarmerUserAuthenticationService userManager)
    {
        _logger = logger;
        _groundProvider = groundProvider;
        _userManager = userManager;

        _groundProvider.NewDevicePosition += NewDevicePositionReceived;
        _groundProvider.NewPlantInGround += NewPlantInGroundReceived;
        _groundProvider.NewPlan += NewPlanReceived;
        _groundProvider.NewAutoIrrigationPlan += NewAutoIrrigationPlanReceived;
        _groundProvider.NewAlert += NewAlertReceived;
        _groundProvider.NewAlertStatus += NewAlertStatusReceived;
    }

    public async Task NotifyPosition(string userId, FarmerDevicePositionRequestData position)
    {
        await _groundProvider.NotifyDevicePosition(userId, position);
    }

    public async Task NewPositionReceived(FarmerDevicePosition position)
        => await 
            Clients
                .Group(position.GroundId)
                .SendAsync(HubConstants.NewPositionReceivedMessage, position);

    public async Task NewPlantInGroundReceived(string groundId, string plantInstanceId)
        => await 
            Clients
                .Group(groundId)
                .SendAsync(HubConstants.NewPlantInGround, plantInstanceId);
        
    public async Task NewPlanReceived(string groundId, string planId)
        => await 
            Clients
                .Group(groundId)
                .SendAsync(HubConstants.NewPlan, planId);

    public async Task NewAutoIrrigationPlanReceived(string groundId, string planId)
        => await 
            Clients
                .Group(groundId)
                .SendAsync(HubConstants.NewAutoIrrigationPlan, planId);
        
    public async Task NewAlertReceived(string groundId, string alertId)
        => await 
            Clients
                .Group(groundId)
                .SendAsync(HubConstants.NewAlert, alertId);

    public async Task NewAlertStatusReceived(string groundId, string alertId, bool alertRead)
        => await 
            Clients
                .Group(groundId)
                .SendAsync(HubConstants.AlertStatusChanged, alertId, alertRead);

    public async Task ReceiveCliCommand(string groundId, string command)
    {
        var userId = GetUserIdByConnectionId(Context.ConnectionId);

        if (string.IsNullOrEmpty(userId))
        {
            //TODO return error message
            return;
        }

        await 
            Clients
                .User(userId)
                .SendAsync(HubConstants.ReceiveCliCommand, userId, groundId, command);

    }

    public async Task ReceiveCliCommandResult(string groundId, string commandResult)
    {
        var userId = GetUserIdByConnectionId(Context.ConnectionId);

        if (string.IsNullOrEmpty(userId))
        {
            //TODO return error message
            return;
        }

        await 
            Clients
                .User(GetUserIdByConnectionId(Context.ConnectionId))
                .SendAsync(HubConstants.ReceiveCliCommandResult, userId, groundId, commandResult);
    }

    protected override void Dispose(bool disposing)
    {
        if (_groundProvider != null)
        {
            _groundProvider.NewDevicePosition -= NewDevicePositionReceived;
            _groundProvider.NewPlantInGround -= NewPlantInGroundReceived;
            _groundProvider.NewPlan -= NewPlanReceived;
            _groundProvider.NewAutoIrrigationPlan -= NewAutoIrrigationPlanReceived;
            _groundProvider.NewAlert -= NewAlertReceived;
            _groundProvider.NewAlertStatus -= NewAlertStatusReceived;
        }
    }

    private string GetUserIdByConnectionId(string connectionId)
    {
        return null;
    }

    private void NewDevicePositionReceived(object sender, DevicePositionEventArgs e)
    {
        SmartFarmerLog.Debug($"new position received on ground {e.Position.GroundId}: ({e.Position})");
        Task.Run(async () => await NewPositionReceived(e.Position));
    }

    private void NewPlantInGroundReceived(object sender, NewPlantEventArgs e)
    {
        SmartFarmerLog.Debug($"new plant on ground {e.FarmerGroundId}: ({e.PlantInstanceId})");
        Task.Run(async () => await NewPlantInGroundReceived(e.FarmerGroundId, e.PlantInstanceId));
    }

    private void NewPlanReceived(object sender, NewPlanEventArgs e)
    {
        SmartFarmerLog.Debug($"new plan on ground {e.FarmerGroundId}: ({e.PlanId})");
        Task.Run(async () => await NewPlanReceived(e.FarmerGroundId, e.PlanId));
    }

    private void NewAutoIrrigationPlanReceived(object sender, NewPlanEventArgs e)
    {
        SmartFarmerLog.Debug($"new auto irrigation plan on ground {e.FarmerGroundId}: ({e.PlanId})");
        Task.Run(async () => await NewAutoIrrigationPlanReceived(e.FarmerGroundId, e.PlanId));
    }

    private void NewAlertReceived(object sender, NewAlertEventArgs e)
    {
        SmartFarmerLog.Debug($"new alert on ground {e.FarmerGroundId}: ({e.AlertId})");
        Task.Run(async () => await NewAlertReceived(e.FarmerGroundId, e.AlertId));
    }
    
    private void NewAlertStatusReceived(object sender, NewAlertStatusEventArgs e)
    {
        SmartFarmerLog.Debug($"new alert status: ({e.AlertId}): read status {e.AlertRead}");
        Task.Run(async () => await NewAlertStatusReceived(e.FarmerGroundId, e.AlertId, e.AlertRead));
    }
}