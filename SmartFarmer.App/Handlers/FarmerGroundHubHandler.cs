using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;

namespace SmartFarmer.Handlers;

public class FarmerGroundHubHandler
{
    private HubConnection _connection;
    private HubConnectionConfiguration _hubConfiguration;
    private IFarmerGround _ground;

    public FarmerGroundHubHandler(IFarmerGround ground, HubConnectionConfiguration hubConfiguration)
    {
        if (hubConfiguration == null) throw new ArgumentNullException(nameof(hubConfiguration));
        if (string.IsNullOrEmpty(hubConfiguration.Url)) throw new InvalidProgramException("invalid specified URL");

        _hubConfiguration = hubConfiguration;
        _ground = ground;
    }

    public async Task Prepare()
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

        _connection.On<string>(
            FarmerHubConstants.ON_NEW_POSITION_RECEIVED, 
            (positionStr) => 
            {
                var position = positionStr.Deserialize<FarmerDevicePositionInTime>();

                SmartFarmerLog.Debug(position.ToString());
            });
        
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

    private async Task Handshake(CancellationToken token)
    {
        await _connection.InvokeAsync(FarmerHubConstants.SUBSCRIBE, _ground.ID);
    }
}