
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Configurations;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Handlers;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Position;

namespace SmartFarmer.DeviceManagers;

/// <summary>
/// Implements a proxy pattern towards an external device (e.g. Arduino)
/// </summary>
public class ExternalDeviceProxy : 
    IFarmerDeviceManager,
    IDisposable
{
    private IFarmerGarden _garden;
    private FarmerDevicePositionsRequestData _positionsToSend;
    private FarmerGardenHubHandler _hub;
    private FarmerGardenSerialHandler _serial;

    public ExternalDeviceProxy(
        IFarmerGarden garden, 
        SerialCommunicationConfiguration serialConfiguration,
        HubConnectionConfiguration hubConfiguration)
    {
        _garden = garden;
        
        DevicePosition = new Farmer5dPoint();
        DevicePosition.NewPoint += NewPointReceived;

        _positionsToSend = new FarmerDevicePositionsRequestData()
        {
            GardenId = garden.ID
        };

        ConfigureHub(hubConfiguration);
        ConfigureSerialComm(serialConfiguration);
    }

    public event EventHandler NewPoint;
    public Farmer5dPoint DevicePosition { get; }

    public async Task<bool> MoveArmAtHeightAsync(double heightInCm, CancellationToken token)
    {
        if (heightInCm.IsNan())
        {
            SmartFarmerLog.Warning($"Height is not defined. Skipping movement");
            return false;
        }

        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.MOVE_TO_HEIGHT_COMMAND,
            new object[] { heightInCm });
        
        if (!result.IsSuccess ||
            !double.TryParse(result.Result, out var receivedHeight))
        {
            return false;
        }

        DevicePosition.Z = heightInCm;
        return true;
    }

    public async Task<double> MoveArmAtMaxHeightAsync(CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.MOVE_TO_MAX_HEIGHT_COMMAND,
            null);
        
        if (!result.IsSuccess ||
            !double.TryParse(result.Result, out var receivedHeight))
        {
            return -1;
        }

        DevicePosition.Z = receivedHeight;
        return receivedHeight;
    }

    public async Task<bool> MoveOnGridAsync(double x, double y, CancellationToken token)
    {
        if (x.IsNan() || y.IsNan())
        {
            SmartFarmerLog.Warning($"Grid position is not completely defined {x}/{y}. Skipping movement");
            return false;
        }

        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.MOVE_XY_COMMAND,
            new object[] { x, y });
        
        if (!result.IsSuccess ||
            !int.TryParse(result.Result, out var receivedResultCode))
        {
            return false;
        }

        return receivedResultCode > 0;
    }

    public async Task<bool> PointDeviceAsync(double degrees, CancellationToken token)
    {
        if (degrees.IsNan())
        {
            SmartFarmerLog.Warning($"Device angle is not defined. Skipping movement");
            return false;
        }

        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.TURN_VERTICAL_COMMAND,
            new object[] { degrees });
           
        if (!result.IsSuccess ||
            !double.TryParse(result.Result, out var receivedDegrees))
        {
            return false;
        }

        DevicePosition.Beta = receivedDegrees;
        return true;
    }

    public async Task<bool> TurnArmToDegreesAsync(double degrees, CancellationToken token)
    {
        if (degrees.IsNan())
        {
            SmartFarmerLog.Warning($"Turning angle is not defined. Skipping movement");
            return false;
        }

        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.TURN_VERTICAL_COMMAND,
            new object[] { degrees });
                
        if (!result.IsSuccess ||
            !double.TryParse(result.Result, out var receivedDegrees))
        {
            return false;
        }

        DevicePosition.Alpha = receivedDegrees;
        return true;
    }

    public async Task<bool> MoveToPosition(Farmer5dPoint position, CancellationToken token)
    {
        bool moveResult = false;

        moveResult = await MoveOnGridAsync(position.X, position.Y, token);
        moveResult |= await MoveArmAtHeightAsync(position.Z, token);
        moveResult |= await TurnArmToDegreesAsync(position.Alpha, token);
        moveResult |= await PointDeviceAsync(position.Beta, token);

        return moveResult;
    }

    public async Task<double> GetCurrentHumidityLevel(CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.GET_HUMIDITY_LEVEL, null);
        
        if (!result.IsSuccess ||
            !double.TryParse(result.Result, out var amount))
        {
            return -1;
        }

        return amount;
    }

    public async Task<bool> ProvideWaterAsync(int pumpNumber, double amountInLiters, CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.HANDLE_PUMP_COMMAND, 
            new object[] { pumpNumber, amountInLiters });
        
        if (!result.IsSuccess ||
            !int.TryParse(result.Result, out var amount))
        {
            return false;
        }

        return amount >= 0;
    }

    public void Dispose()
    {
        DevicePosition.NewPoint -= NewPointReceived;

        try
        {
            if (_serial != null)
            {
                _serial.PartialResultReceived += ProcessRequestUpdateResult;
                _serial.Dispose();
            }
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
        }
    }

    private void NewPointReceived(object sender, EventArgs args)
    {
        var newPosition = new FarmerDevicePositionInTime(DevicePosition)
        {
            PositionDt = DateTime.UtcNow
        };

        Task.Run(async () => await NotifyNewReceivedPosition(newPosition, sender, args));
    }

    private async Task NotifyNewReceivedPosition(FarmerDevicePositionInTime position, object sender, EventArgs args)
    {
        if (sender is IFarmerPointNotifier notifier)
        {
            NewPoint.Invoke(this, args);
        }

        try
        {
            // send on hub immediately
            await _hub.SendDevicePosition(
                new FarmerDevicePositionRequestData()
                {
                    GardenId = _garden.ID,
                    RunId = position.RunId,
                    PositionDt = position.PositionDt,
                    Position = new Farmer5dPoint(position)
                },
                CancellationToken.None
                );
            SmartFarmerLog.Debug($"position sent through hub");
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);

            // if sending fails, collect to future tries
            _positionsToSend.Positions.Add(position);
        }
    }

    private void ConfigureHub(HubConnectionConfiguration hubConfiguration)
    {
        _hub = new FarmerGardenHubHandler(_garden, hubConfiguration);

        Task.Run(async () => {
            try
            {
                await _hub.InitializeAsync(CancellationToken.None);
            }
            catch (AggregateException ec)
            {
                SmartFarmerLog.Exception(ec);
            }
        });
    }
    
    private void ConfigureSerialComm(SerialCommunicationConfiguration serialConfiguration)
    {
        _serial = new FarmerGardenSerialHandler(serialConfiguration);

        _serial.PartialResultReceived += ProcessRequestUpdateResult;
    }

    private async Task<SerialCommandFinalResult> SendCommandToExternalDevice(
        string command,
        object[] parameters)
    {
        return await _serial.SendCommandToExternalDevice(command, parameters);
    }

    private int ProcessRequestFinalResult(SerialCommandFinalResult args)
    {
        int result;
        SmartFarmerLog.Debug($"received message from serial port:\n\t->{args.Result.Replace("\r\n", "").Replace("\n", "")}");
        result = GetReceivedValueOutcome(args.ExpectedRequestId, args.Result);

        if (result == -1)
        {
            SmartFarmerLog.Error($"invalid response for command: \"{args.Command}\". Received {args.Result}");
            SendDeviceError(args.ExpectedRequestId, args.Command, args.Result);
        }
        else
        {
            SendDevicePositionHistory();
        }

        return result;
    }

    private void SendDevicePositionHistory()
    {
        if (_positionsToSend == null ||
            _positionsToSend.Positions.IsNullOrEmpty())
        {
            SmartFarmerLog.Debug($"position history is empty");
            return;
        }

        SmartFarmerLog.Debug($"position history contains {_positionsToSend.Positions.Count} positions");
        Task.Run(async () => await FarmerRequestHandler.NotifyDevicePositions(_positionsToSend, CancellationToken.None));

        var lastPos = 
            _positionsToSend
                .Positions
                    .OrderByDescending(x => x.PositionDt)
                    .First();

        Task.Run(async () => await _hub.NotifyDevicePosition(_positionsToSend.GardenId, lastPos, CancellationToken.None));
    }

    private void SendDeviceError(string requestId, string command, string receivedValue)
    {
        //TODO notify device error
    }

    private void ProcessRequestUpdateResult(object sender, SerialCommandPartialResultEventArgs args)
    {
        if (args.RequestId != args.ExpectedRequestId)
        {
            return;
        }

        NotifyPartialResultByCommand(args.Command, args.Result);
    }

    private void NotifyPartialResultByCommand(string command, string resultStr)
    {
        SmartFarmerLog.Debug($"Partial update for command {command}: received {resultStr}");

        switch (command)
        {
            case ExternalDeviceProtocolConstants.MOVE_XY_COMMAND:
                {
                    var parameters = SerialCommandUtils.DeserializeParameters(resultStr);
                    if (parameters != null && parameters.Length >= 2)
                    {
                        if (double.TryParse(parameters[0], out var currentX))
                        {
                            DevicePosition.X = currentX;
                        }

                        if (double.TryParse(parameters[1], out var currentY))
                        {
                            DevicePosition.Y = currentY;
                        }
                    }
                }
                break;

            case ExternalDeviceProtocolConstants.MOVE_TO_HEIGHT_COMMAND:
            case ExternalDeviceProtocolConstants.MOVE_TO_MAX_HEIGHT_COMMAND:
                {
                    if (double.TryParse(resultStr, out var currentZ))
                    {
                        DevicePosition.Z = currentZ;
                    }
                }
                
                break;
        }
    }

    private int GetReceivedValueOutcome(string expectedRequestId, string receivedValue)
    {
        SerialCommandUtils.ParseResponse(
            receivedValue, 
            out var requestId, 
            out var resultStr);

        // SmartFarmerLog.Debug($"expected {expectedRequestId}, found {requestId}. Outcome {resultStr}");

        if (requestId == expectedRequestId && 
            int.TryParse(resultStr, out var result))
        {
            return result;
        }

        return -1;
    }
}
