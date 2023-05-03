using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;

namespace SmartFarmer.OperationalManagement;

public class RemoteCommandLineInterfaceOperationalManager : IOperationalModeManager
{
    private HubConnection _connection;
    private HubConnectionConfiguration _hubConfiguration;

    public RemoteCommandLineInterfaceOperationalManager(HubConnectionConfiguration hubConfiguration)
    {
        _hubConfiguration = hubConfiguration;
    }

    public AppOperationalMode Mode => AppOperationalMode.RemoteCLI;
    public string Name => "Remote CLI";
    public event EventHandler<OperationRequestEventArgs> NewOperationRequired;


    public async Task Prepare()
    {
        // Opening SignalR connection
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubConfiguration.Url)
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
        _connection.On<string, string, string>("ReceiveCliCommand", (user, groundId, command) =>
        {
            SmartFarmerLog.Debug($"{user}: {command} for ground {groundId}");

            ParseAndExecuteCommand(user, groundId, command);
        });

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
        }
    }

    private void ParseAndExecuteCommand(string user, string groundId, string command)
    {
        var parts = command.Split(' ');

        //TODO Check parts
        //TODO send action

        // notify result
        Task.Run(async () => await NotifyResult(user, groundId, command, "command result"));
    }

    private async Task NotifyResult(string user, string groundId, string command, string result)
    {
        try
        {
            await _connection.InvokeAsync(
                "ReceiveCLICommandResult", 
                result);
        }
        catch (Exception ex)
        {                
            SmartFarmerLog.Exception(ex);
        }
    }

    public void Dispose()
    {
        // Closing SignalR connection
        if (_connection != null)
        {
            Task.Run(async () => await _connection.DisposeAsync());
        }
    }
}
