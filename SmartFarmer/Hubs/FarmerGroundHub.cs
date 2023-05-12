using System;
using System.Threading.Tasks;
using SmartFarmer.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartFarmer.Controllers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Services;
using System.Text.Json;
using System.Linq;
using SmartFarmer.DTOs.Alerts;
using SmartFarmer.Tasks;

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

    public async Task SendNewAlertStatusAsync(string alertId, bool alertRead)
    {
        var result = await _groundProvider.MarkFarmerAlertAsRead(Context.UserIdentifier, alertId, alertRead);

        if (result)
        {
            await NotifyNewAlertStatusAsync(alertId, alertRead);
        }
    }

    public async Task NotifyNewAlertStatusAsync(string alertId, bool alertRead)
    {
        var alert =
            (await _groundProvider.GetFarmerAlertsByIdAsync(Context.UserIdentifier, new[] { alertId }))
                ?.FirstOrDefault()
                as FarmerAlert;

        if (alert == null)
        {
            SmartFarmerLog.Error($"Invalid alert for {alertId} and user {Context.UserIdentifier}");
            return;
        }

        await
            Clients
                .OthersInGroup(alert.FarmerGroundId)
                .SendAsync(HubConstants.AlertStatusChanged, alertId, alertRead);
    }

    public async Task SendCliCommandAsync(string groundId, string commandStr)
    {
        var userId = Context.UserIdentifier;

        IFarmerCliCommand command = _groundProvider.BuildAndCheckCliCommand(userId, groundId, commandStr);

        if (command == null)
        {
            SmartFarmerLog.Error("Invalid command for string " + commandStr);
            return;
        }

        await
            Clients
                .OthersInGroup(groundId)
                .SendAsync(HubConstants.NewCliCommand, JsonSerializer.Serialize(command));
    }

    public async Task NotifyCliCommandResult(string groundId, string commandResult)
    {
        await 
            Clients
                .OthersInGroup(groundId)
                .SendAsync(HubConstants.ReceiveCliCommandResult, commandResult);
    }

    private async Task NotifyNewPositionAsync(string groundId, FarmerDevicePositionInTime position)
    {
        await 
            Clients
                .OthersInGroup(groundId)
                .SendAsync(HubConstants.NewPositionReceivedMessage, JsonSerializer.Serialize(position));
    }
}