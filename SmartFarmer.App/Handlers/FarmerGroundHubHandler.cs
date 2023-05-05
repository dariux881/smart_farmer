using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SmartFarmer.Data;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;

namespace SmartFarmer.Handlers;

public class FarmerGroundHubHandler
{
    private HubConnection _connection;
    private HubConnectionConfiguration _hubConfiguration;
    private FarmerGround _ground;

    public FarmerGroundHubHandler(FarmerGround ground, HubConnectionConfiguration hubConfiguration)
    {
        _hubConfiguration = hubConfiguration;
        _ground = ground;
    }

    public async Task Prepare()
    {
        // Opening SignalR connection
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubConfiguration.Url)
            .WithAutomaticReconnect()
            .Build();

        _connection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0,5) * 1000);
            await _connection.StartAsync();
        };

        await Task.CompletedTask;
    }
    
    public async Task Run(CancellationToken token)
    {
        // Start connection and subscribe to commands
        // _connection.On<string, string, string>("ReceiveCliCommand", (user, groundId, command) =>
        // {
        //     SmartFarmerLog.Debug($"{user}: {command} for ground {groundId}");
        //     ParseAndExecuteCommand(user, groundId, command);
        // });

        try
        {
            await _connection.StartAsync();
            await Handshake(token);
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
        }
    }
    
    private async Task Handshake(CancellationToken token)
    {
        await _connection.InvokeAsync("SubscribeToGroups", LocalConfiguration.LoggedUserId, _ground.ID);
    }

}