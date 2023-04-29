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
    }

    private void NewDevicePositionReceived(object sender, DevicePositionEventArgs e)
    {
        Task.Run(async () => await NewPositionReceived(e.Position));
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
}