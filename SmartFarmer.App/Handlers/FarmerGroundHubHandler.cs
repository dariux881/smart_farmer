using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;

namespace SmartFarmer.Handlers;

public class FarmerGroundHubHandler : IAsyncDisposable
{
    private HubConnection _connection;
    private HubConnectionConfiguration _hubConfiguration;
    private IFarmerGround _ground;

    public FarmerGroundHubHandler(
        IFarmerGround ground, 
        HubConnectionConfiguration hubConfiguration)
    {
        if (hubConfiguration == null) throw new ArgumentNullException(nameof(hubConfiguration));
        if (string.IsNullOrEmpty(hubConfiguration.Url)) throw new InvalidProgramException("invalid specified URL");

        _hubConfiguration = hubConfiguration;
        _ground = ground;
    }

    public string SubscribedGroundId => _ground?.ID;

    public event EventHandler<DevicePositionEventArgs> NewDevicePositionReceived;
    public event EventHandler<NewAlertStatusEventArgs> NewAlertStatusEventArgsReceived;
    public event EventHandler<NewCliCommandEventArgs> NewCliCommandReceived;

    public async Task InitializeAsync()
    {
        // Opening SignalR connection
        _connection = new HubConnectionBuilder()
            .WithUrl(
                _hubConfiguration.Url, 
                options => {
                    options.AccessTokenProvider = () => Task.FromResult(LocalConfiguration.Token);
                    options.Headers.Add(
                        SmartFarmerApiConstants.USER_AUTHENTICATION_HEADER_KEY, 
                        LocalConfiguration.Token);
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
            await _connection.StartAsync();
        };

        SubscribeToNotificationMethods();
        
        try
        {
            await _connection.StartAsync();
            await Handshake(CancellationToken.None);
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
            FarmerHubConstants.ON_NEW_POSITION_RECEIVED, 
            (positionStr) => 
            {
                NewDevicePositionReceived?.Invoke(this, new DevicePositionEventArgs(positionStr));
            });
        
        _connection.On<string, bool>(
            FarmerHubConstants.ON_ALERT_STATUS_CHANGED,
            (alertId, status) => {
                NewAlertStatusEventArgsReceived?.Invoke(this, new NewAlertStatusEventArgs(alertId, status));
            });

        _connection.On<string>(
            FarmerHubConstants.NEW_CLI_COMMAND,
            (command) => {
                NewCliCommandReceived?.Invoke(this, new NewCliCommandEventArgs(command));
            });
    }

    public async Task SendDevicePosition(FarmerDevicePositionRequestData position)
    {
        if (position == null) throw new ArgumentNullException(nameof(position));

        await _connection.InvokeAsync(
            FarmerHubConstants.INSERT_DEVICE_POSITION,
            position.Serialize());
    }

    public async Task NotifyDevicePosition(string groundId, FarmerDevicePositionInTime position)
    {
        if (position == null) throw new ArgumentNullException(nameof(position));

        await _connection.InvokeAsync(
            FarmerHubConstants.NOTIFY_DEVICE_POSITION,
            groundId,
            position.Serialize());
    }

    public async Task ChangeAlertStatus(string alertId, bool alertStatus)
    {
        if (alertId == null) throw new ArgumentNullException(nameof(alertId));

        await _connection.InvokeAsync(
            FarmerHubConstants.CHANGE_ALERT_STATUS,
            alertId,
            alertStatus);
    }

    public async Task NotifyNewAlertStatus(string alertId, bool status)
    {
        if (alertId == null) throw new ArgumentNullException(nameof(alertId));

        await _connection.InvokeAsync(
            FarmerHubConstants.NOTIFY_NEW_ALERT_STATUS,
            alertId,
            status);
    }

    public async Task SendCliCommandAsync(string groundId, string command)
    {
        if (groundId == null) throw new ArgumentNullException(nameof(groundId));

        await _connection.InvokeAsync(
            FarmerHubConstants.SEND_CLI_COMMAND,
            groundId,
            command);
    }

    public async Task NotifyCliCommandResult(string groundId, string result)
    {
        if (groundId == null) throw new ArgumentNullException(nameof(groundId));

        await _connection.InvokeAsync(
            FarmerHubConstants.NOTIFY_CLI_COMMAND_RESULT,
            groundId,
            result);
    }

    private async Task Handshake(CancellationToken token)
    {
        if (_ground == null) return;

        await _connection.InvokeAsync(FarmerHubConstants.SUBSCRIBE, _ground.ID);
    }
}