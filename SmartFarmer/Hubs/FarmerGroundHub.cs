using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartFarmer.Controllers;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.Movement;
using SmartFarmer.Services;

namespace SmartFarmer.Hubs;

public class FarmerGroundHub : Hub
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

    private void NewDevicePositionReceived(object sender, DevicePositionEventArgs e)
    {
        Task.Run(async () => await NewPositionReceived(e.Position));
    }

    private void NewPlantInGroundReceived(object sender, NewPlantEventArgs e)
    {
        Task.Run(async () => await NewPlantInGroundReceived(e.FarmerGroundId, e.PlantInstanceId));
    }

    private void NewPlanReceived(object sender, NewPlanEventArgs e)
    {
        Task.Run(async () => await NewPlanReceived(e.FarmerGroundId, e.PlanId));
    }

    private void NewAlertReceived(object sender, NewAlertEventArgs e)
    {
        Task.Run(async () => await NewAlertReceived(e.FarmerGroundId, e.AlertId));
    }
    
    private void NewAlertStatusReceived(object sender, NewAlertStatusEventArgs e)
    {
        Task.Run(async () => await NewAlertStatusReceived(e.FarmerGroundId, e.AlertId, e.AlertRead));
    }
}