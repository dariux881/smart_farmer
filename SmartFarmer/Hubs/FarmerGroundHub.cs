using System;
using System.Threading.Tasks;
using SmartFarmer.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartFarmer.Controllers;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Services;
using System.Text.Json;

namespace SmartFarmer.Hubs;

[Authorize]
public class FarmerGroundHub : Hub
{
    private readonly ILogger<FarmerGroundController> _logger;
    private readonly ISmartFarmerGroundControllerService _groundProvider;

    public FarmerGroundHub(
        ILogger<FarmerGroundController> logger,
        ISmartFarmerGroundControllerService groundProvider)
    {
        _logger = logger;
        _groundProvider = groundProvider;
    }

    public async Task AddToGroupAsync(string groundId)
    {
        if (string.IsNullOrEmpty(groundId)) throw new ArgumentNullException(nameof(groundId));

        var connectionId = Context.ConnectionId;
        if (!string.IsNullOrEmpty(groundId))
        {
            // subscribing to groups
            await Groups.AddToGroupAsync(Context.ConnectionId, groundId);
        }
    }

    public async Task RemoveFromGroupAsync(string groundId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groundId);
    }

    public async Task InsertNewPositionAsync(string positionStr)
    {
        var userId = Context.UserIdentifier;
        var position = JsonSerializer.Deserialize<FarmerDevicePositionRequestData>(positionStr);

        var result = await _groundProvider.NotifyDevicePosition(userId, position);

        if (result != null)
        {
            await NotifyNewPositionAsync(result.GroundId, result);
        }
    }

    public async Task NotifyNewPositionAsync(string groundId, string positionStr)
    {
        var position = JsonSerializer.Deserialize<FarmerDevicePositionInTime>(positionStr);
        await NotifyNewPositionAsync(groundId, position);
    }

    public async Task NotifyNewPlanAsync(string groundId, string planId)
        => await 
            Clients
                .OthersInGroup(groundId)
                .SendAsync(HubConstants.NewPlan, planId);

    public async Task NotifyNewAutoIrrigationPlanAsync(string groundId, string planId)
        => await 
            Clients
                .OthersInGroup(groundId)
                .SendAsync(HubConstants.NewAutoIrrigationPlan, planId);
        
    public async Task NotifyNewAlertAsync(string groundId, string alertId)
        => await 
            Clients
                .OthersInGroup(groundId)
                .SendAsync(HubConstants.NewAlert, alertId);

    public async Task NotifyNewAlertStatusAsync(string groundId, string alertId, bool alertRead)
        => await 
            Clients
                .OthersInGroup(groundId)
                .SendAsync(HubConstants.AlertStatusChanged, alertId, alertRead);

    public async Task ReceiveCliCommand(string groundId, string command)
    {
        var userId = Context.UserIdentifier;

        //TODO validate command, parse it, then send the result
        
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
        var userId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(userId))
        {
            //TODO return error message
            return;
        }

        await 
            Clients
                .User(Context.UserIdentifier)
                .SendAsync(HubConstants.ReceiveCliCommandResult, userId, groundId, commandResult);
    }

    private async Task NotifyNewPositionAsync(string groundId, FarmerDevicePositionInTime position)
    {
        await 
            Clients
                .OthersInGroup(groundId)
                .SendAsync(HubConstants.NewPositionReceivedMessage, JsonSerializer.Serialize(position));
    }
}