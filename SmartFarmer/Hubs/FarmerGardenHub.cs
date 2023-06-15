using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartFarmer.Controllers;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Services;
using SmartFarmer.DTOs.Alerts;
using SmartFarmer.Tasks;

namespace SmartFarmer.Hubs;

[Authorize]
public class FarmerGardenHub : Hub
{
    private readonly ILogger<FarmerGardenController> _logger;
    private readonly ISmartFarmerGardenControllerService _gardenProvider;

    public FarmerGardenHub(
        ILogger<FarmerGardenController> logger,
        ISmartFarmerGardenControllerService gardenProvider)
    {
        _logger = logger;
        _gardenProvider = gardenProvider;
    }

    public async Task AddToGroupAsync(string gardenId)
    {
        if (string.IsNullOrEmpty(gardenId)) throw new ArgumentNullException(nameof(gardenId));

        var connectionId = Context.ConnectionId;
        if (!string.IsNullOrEmpty(gardenId))
        {
            // subscribing to groups
            await Groups.AddToGroupAsync(Context.ConnectionId, gardenId);
        }
    }

    public async Task RemoveFromGroupAsync(string gardenId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gardenId);
    }

    public async Task InsertNewPositionAsync(string positionStr)
    {
        var userId = Context.UserIdentifier;
        var position = JsonSerializer.Deserialize<FarmerDevicePositionRequestData>(positionStr);

        var result = await _gardenProvider.NotifyDevicePosition(userId, position);

        if (result != null)
        {
            await NotifyNewPositionAsync(result.GardenId, result);
        }
    }

    public async Task NotifyNewPositionAsync(string gardenId, string positionStr)
    {
        var position = JsonSerializer.Deserialize<FarmerDevicePositionInTime>(positionStr);
        await NotifyNewPositionAsync(gardenId, position);
    }

    public async Task NotifyNewPlanAsync(string gardenId, string planId)
        => await 
            Clients
                .OthersInGroup(gardenId)
                .SendAsync(HubConstants.NewPlan, planId);

    public async Task NotifyDeletedPlanAsync(string gardenId, string planId)
        => await 
            Clients
                .OthersInGroup(gardenId)
                .SendAsync(HubConstants.DeletedPlan, planId);

    public async Task NotifyNewAutoIrrigationPlanAsync(string gardenId, string planId)
        => await 
            Clients
                .OthersInGroup(gardenId)
                .SendAsync(HubConstants.NewAutoIrrigationPlan, planId);
        
    public async Task NotifyNewAlertAsync(string gardenId, string alertId)
        => await 
            Clients
                .OthersInGroup(gardenId)
                .SendAsync(HubConstants.NewAlert, alertId);

    public async Task SendNewAlertStatusAsync(string alertId, bool alertRead)
    {
        var result = await _gardenProvider.MarkFarmerAlertAsRead(Context.UserIdentifier, alertId, alertRead);

        if (result)
        {
            await NotifyNewAlertStatusAsync(alertId, alertRead);
        }
    }

    public async Task NotifyNewAlertStatusAsync(string alertId, bool alertRead)
    {
        var alert =
            (await _gardenProvider.GetFarmerAlertsByIdAsync(Context.UserIdentifier, new[] { alertId }))
                ?.FirstOrDefault()
                as FarmerAlert;

        if (alert == null)
        {
            SmartFarmerLog.Error($"Invalid alert for {alertId} and user {Context.UserIdentifier}");
            return;
        }

        await
            Clients
                .OthersInGroup(alert.FarmerGardenId)
                .SendAsync(HubConstants.AlertStatusChanged, alertId, alertRead);
    }

    public async Task SendCliCommandAsync(string gardenId, string commandStr)
    {
        var userId = Context.UserIdentifier;

        IFarmerCliCommand command = _gardenProvider.BuildAndCheckCliCommand(userId, gardenId, commandStr);

        if (command == null)
        {
            SmartFarmerLog.Error("Invalid command for string " + commandStr);
            return;
        }

        await
            Clients
                .OthersInGroup(gardenId)
                .SendAsync(HubConstants.NewCliCommand, JsonSerializer.Serialize(command));
    }

    public async Task NotifyCliCommandResult(string gardenId, string commandResult)
    {
        await 
            Clients
                .OthersInGroup(gardenId)
                .SendAsync(HubConstants.ReceiveCliCommandResult, commandResult);
    }

    private async Task NotifyNewPositionAsync(string gardenId, FarmerDevicePositionInTime position)
    {
        await 
            Clients
                .OthersInGroup(gardenId)
                .SendAsync(HubConstants.NewPositionReceivedMessage, JsonSerializer.Serialize(position));
    }
}