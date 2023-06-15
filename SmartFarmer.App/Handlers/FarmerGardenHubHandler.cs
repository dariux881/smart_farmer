using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SmartFarmer.Configurations;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;

namespace SmartFarmer.Handlers;

public class FarmerGardenHubHandler : IAsyncDisposable
{
    private HubConnection _connection;
    private HubConnectionConfiguration _hubConfiguration;
    private IFarmerGarden _garden;
    private readonly IFarmerSessionManager _sessionManager;

    public FarmerGardenHubHandler(
        IFarmerGarden garden, 
        HubConnectionConfiguration hubConfiguration)
    {
        if (hubConfiguration == null) throw new ArgumentNullException(nameof(hubConfiguration));
        if (string.IsNullOrEmpty(hubConfiguration.Url)) throw new InvalidProgramException("invalid specified URL");

        _hubConfiguration = hubConfiguration;
        _garden = garden;

        _sessionManager = FarmerServiceLocator.GetService<IFarmerSessionManager>(true);
    }

    public string SubscribedGardenId => _garden?.ID;

    public event EventHandler<DevicePositionEventArgs> NewDevicePositionReceived;
    public event EventHandler<NewAlertStatusEventArgs> NewAlertStatusEventArgsReceived;
    public event EventHandler<NewCliCommandEventArgs> NewCliCommandReceived;
    public event EventHandler<CliCommandResultEventArgs> CliCommandResultReceived;

    public async Task InitializeAsync(CancellationToken token)
    {
        // Opening SignalR connection
        _connection = new HubConnectionBuilder()
            .WithUrl(
                _hubConfiguration.Url, 
                options => {
                    options.AccessTokenProvider = () => Task.FromResult(_sessionManager.Token);
                    options.Headers.Add(
                        SmartFarmerApiConstants.USER_AUTHENTICATION_HEADER_KEY, 
                        _sessionManager.Token);
                }
            )
            // .ConfigureLogging(logging => 
            // {
            //     logging.AddConsole();
            //     logging.SetMinimumLevel(LogLevel.Warning);
            // })
            .WithAutomaticReconnect()
            .Build();

        _connection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0,5) * 1000);
            await _connection.StartAsync(token);
        };

        SubscribeToNotificationMethods();
        
        try
        {
            await _connection.StartAsync(token);
            await Handshake(token);
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    private void SubscribeToNotificationMethods()
    {
        _connection.On<string>(
            FarmerHubConstants.RECEIVE_DEVICE_POSITION, 
            (positionStr) => 
            {
                NewDevicePositionReceived?.Invoke(this, new DevicePositionEventArgs(positionStr));
            });
        
        _connection.On<string, bool>(
            FarmerHubConstants.RECEIVE_ALERT_STATUS_CHANGE,
            (alertId, status) => {
                NewAlertStatusEventArgsReceived?.Invoke(this, new NewAlertStatusEventArgs(alertId, status));
            });

        _connection.On<string>(
            FarmerHubConstants.RECEIVE_CLI_COMMAND,
            (command) => {
                NewCliCommandReceived?.Invoke(this, new NewCliCommandEventArgs(command));
            });

        _connection.On<string>(
            FarmerHubConstants.RECEIVE_CLI_COMMAND_RESULT,
            (commandResult) => {
                CliCommandResultReceived?.Invoke(this, new CliCommandResultEventArgs(commandResult));
            });
    }

    public async Task SendDevicePosition(FarmerDevicePositionRequestData position, CancellationToken token)
    {
        if (position == null) throw new ArgumentNullException(nameof(position));

        await _connection.InvokeAsync(
            FarmerHubConstants.INSERT_DEVICE_POSITION,
            position.Serialize(),
            token);
    }

    public async Task NotifyDevicePosition(string gardenId, FarmerDevicePositionInTime position, CancellationToken token)
    {
        if (position == null) throw new ArgumentNullException(nameof(position));

        await _connection.InvokeAsync(
            FarmerHubConstants.SEND_DEVICE_POSITION_NOTIFICATION,
            gardenId,
            position.Serialize(),
            token);
    }

    public async Task ChangeAlertStatus(string alertId, bool alertStatus, CancellationToken token)
    {
        if (alertId == null) throw new ArgumentNullException(nameof(alertId));

        await _connection.InvokeAsync(
            FarmerHubConstants.SEND_NEW_ALERT_STATUS,
            alertId,
            alertStatus,
            token);
    }

    public async Task NotifyNewAlertStatus(string alertId, bool status, CancellationToken token)
    {
        if (alertId == null) throw new ArgumentNullException(nameof(alertId));

        await _connection.InvokeAsync(
            FarmerHubConstants.SEND_NEW_ALERT_STATUS_NOTIFICATION,
            alertId,
            status,
            token);
    }

    public async Task SendCliCommandAsync(string gardenId, string command, CancellationToken token)
    {
        if (gardenId == null) throw new ArgumentNullException(nameof(gardenId));

        await _connection.InvokeAsync(
            FarmerHubConstants.SEND_CLI_COMMAND,
            gardenId,
            command,
            token);
    }

    public async Task NotifyCliCommandResult(string gardenId, string result, CancellationToken token)
    {
        if (gardenId == null) throw new ArgumentNullException(nameof(gardenId));

        await _connection.InvokeAsync(
            FarmerHubConstants.SEND_CLI_COMMAND_RESULT,
            gardenId,
            result,
            token);
    }

    private async Task Handshake(CancellationToken token)
    {
        if (_garden == null) return;

        await _connection.InvokeAsync(FarmerHubConstants.SUBSCRIBE, _garden.ID);
    }
}