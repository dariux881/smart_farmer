
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer;
using SmartFarmer.Handlers;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Tasks.Movement;

/// <summary>
/// Implements a proxy pattern towards an external device (e.g. Arduino)
/// </summary>
public class ExternalDeviceProxy : 
    IFarmerDeviceManager,
    IDisposable
{
    private IFarmerGround _ground;
    private Farmer5dPositionNotifier _positionNotifier;
    private FarmerDevicePositionsRequestData _positionsToSend;
    private FarmerGroundHubHandler _hub;
    private FarmerGroundSerialHandler _serial;

    public ExternalDeviceProxy(
        IFarmerGround ground, 
        SerialCommunicationConfiguration serialConfiguration,
        HubConnectionConfiguration hubConfiguration)
    {
        _ground = ground;
        
        _positionNotifier = new Farmer5dPositionNotifier();
        _positionNotifier.NewPoint += NewPointReceived;

        _positionsToSend = new FarmerDevicePositionsRequestData()
        {
            GroundId = ground.ID
        };

        ConfigureHub(hubConfiguration);
        ConfigureSerialComm(serialConfiguration);
    }

    public double X => _positionNotifier.X;
    public double Y => _positionNotifier.Y;
    public double Z => _positionNotifier.Z;
    public double Alpha => _positionNotifier.Alpha;
    public double Beta => _positionNotifier.Beta;

    public event EventHandler NewPoint;

    public async Task<bool> MoveArmAtHeightAsync(double heightInCm, CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.MOVE_TO_HEIGHT_COMMAND,
            new object[] { heightInCm });
        
        if (!result.IsSuccess ||
            !double.TryParse(result.Result, out var receivedHeight))
        {
            return false;
        }

        _positionNotifier.Z = heightInCm;
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

        _positionNotifier.Z = receivedHeight;
        return receivedHeight;
    }

    public async Task<bool> MoveOnGridAsync(double x, double y, CancellationToken token)
    {
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
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.TURN_VERTICAL_COMMAND,
            new object[] { degrees });
           
        if (!result.IsSuccess ||
            !double.TryParse(result.Result, out var receivedDegrees))
        {
            return false;
        }

        _positionNotifier.Beta = receivedDegrees;
        return true;
    }

    public async Task<bool> TurnArmToDegreesAsync(double degrees, CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.TURN_VERTICAL_COMMAND,
            new object[] { degrees });
                
        if (!result.IsSuccess ||
            !double.TryParse(result.Result, out var receivedDegrees))
        {
            return false;
        }

        _positionNotifier.Alpha = receivedDegrees;
        return true;
    }

    public async Task<bool> MoveToPosition(IFarmer5dPoint position, CancellationToken token)
    {
        var moveResult = await MoveOnGridAsync(position.X, position.Y, token);

        if (!moveResult)
        {
            SmartFarmerLog.Error($"Failing in moving on grid to {position.ToString()}");
            return false;
        }

        moveResult = await MoveArmAtHeightAsync(position.Z, token);

        if (!moveResult)
        {
            SmartFarmerLog.Error($"Failing in moving at height to {position.ToString()}");
            return false;
        }

        moveResult = await TurnArmToDegreesAsync(position.Alpha, token);

        if (!moveResult)
        {
            SmartFarmerLog.Error($"Failing in turning arm to {position.ToString()}");
            return false;
        }

        moveResult = await PointDeviceAsync(position.Beta, token);

        if (!moveResult)
        {
            SmartFarmerLog.Error($"Failing in pointing to {position.ToString()}");
            return false;
        }

        return true;
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
        _positionNotifier.NewPoint -= NewPointReceived;

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
        SmartFarmerLog.Information($"new position received: {this._positionNotifier.ToString()}");
        
        var newPosition = new FarmerDevicePositionInTime()
        {
            X = this.X,
            Y = this.Y,
            Z = this.Z,
            Alpha = this.Alpha,
            Beta = this.Beta,
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
                    GroundId = _ground.ID,
                    RunId = position.RunId,
                    PositionDt = position.PositionDt,
                    Position = new Farmer5dPoint(position)
                }
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
        _hub = new FarmerGroundHubHandler(_ground, hubConfiguration);
    }
    
    private void ConfigureSerialComm(SerialCommunicationConfiguration serialConfiguration)
    {
        _serial = new FarmerGroundSerialHandler(serialConfiguration);

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
        if (_positionsToSend?.Positions == null || !_positionsToSend.Positions.Any())
        {
            SmartFarmerLog.Debug($"position history is empty");
            return;
        }

        SmartFarmerLog.Debug($"position history contains {_positionsToSend.Positions.Count} positions");
        Task.Run(async () => await FarmerRequestHelper.NotifyDevicePositions(_positionsToSend, CancellationToken.None));
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
                            _positionNotifier.X = currentX;
                        }

                        if (double.TryParse(parameters[1], out var currentY))
                        {
                            _positionNotifier.Y = currentY;
                        }
                    }
                }
                break;

            case ExternalDeviceProtocolConstants.MOVE_TO_HEIGHT_COMMAND:
            case ExternalDeviceProtocolConstants.MOVE_TO_MAX_HEIGHT_COMMAND:
                {
                    if (double.TryParse(resultStr, out var currentZ))
                    {
                        _positionNotifier.Z = currentZ;
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
