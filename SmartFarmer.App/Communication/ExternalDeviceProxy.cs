
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SmartFarmer;
using SmartFarmer.Exceptions;
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
    private SerialCommunicationConfiguration _serialConfiguration;
    private SerialPort _serialPort;
    private SemaphoreSlim _commandInProgressSem;
    private bool _commandInProgress;

    private int _delayBetweenReadAttempts;
    private int _maxReadAttempts;

    public ExternalDeviceProxy(IFarmerGround ground, SerialCommunicationConfiguration serialConfiguration)
    {
        _ground = ground;
        
        _positionNotifier = new Farmer5dPositionNotifier();
        _positionNotifier.NewPoint += NewPointReceived;

        _serialConfiguration = serialConfiguration;
        _delayBetweenReadAttempts = _serialConfiguration?.DelayBetweenReadAttempts ?? 2000;
        _maxReadAttempts = _serialConfiguration?.MaxReadAttempts ?? 10;

        _commandInProgressSem = new SemaphoreSlim(1);

        ConfigureSerialPort();
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
            new object[] { heightInCm }) >= 0;
        
        if (result)
        {
            _positionNotifier.Z = heightInCm;
        }

        return result;
    }

    public async Task<int> MoveArmAtMaxHeightAsync(CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.MOVE_TO_MAX_HEIGHT_COMMAND,
            null);
        
        var outcome = result >= 0;
        if (outcome)
        {
            _positionNotifier.Z = result;
        }

        return result;
    }

    public async Task<bool> MoveOnGridAsync(double x, double y, CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.MOVE_XY_COMMAND,
            new object[] { x, y }) >= 0;
        
        if (result)
        {
            _positionNotifier.X = x;
            _positionNotifier.Y = y;
        }

        return result;
    }

    public async Task<bool> PointDeviceAsync(double degrees, CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.TURN_VERTICAL_COMMAND,
            new object[] { degrees }) >= 0;
        
        if (result)
        {
            _positionNotifier.Beta = degrees;
        }

        return result;
    }

    public async Task<bool> TurnArmToDegreesAsync(double degrees, CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.TURN_VERTICAL_COMMAND,
            new object[] { degrees }) >= 0;
        
        if (result)
        {
            _positionNotifier.Alpha = degrees;
        }

        return result;
    }

    public async Task<double> GetCurrentHumidityLevel(CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.GET_HUMIDITY_LEVEL, null);
        
        return result;
    }

    public async Task<bool> ProvideWaterAsync(int pumpNumber, double amountInLiters, CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.HANDLE_PUMP_COMMAND, 
            new object[] { pumpNumber, amountInLiters }) >= 0;
        
        return result;
    }

    public void Dispose()
    {
        _positionNotifier.NewPoint -= NewPointReceived;

        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
        }
    }

    private void NewPointReceived(object sender, EventArgs args)
    {
        if (sender is IFarmerPointNotifier notifier)
        {
            NewPoint.Invoke(this, args);
        }

        Task.Run(async () => 
            {
                //TODO store locallly, then send a bundle periodically
                
                SmartFarmerLog.Debug("Sending new position");

                await FarmerRequestHelper.NotifyDevicePosition(
                new FarmerDevicePositionRequestData()
                {
                    GroundId = _ground.ID,
                    PositionDt = DateTime.UtcNow,
                    Position = new Farmer5dPoint(X, Y, Z, Alpha, Beta)
                },
                CancellationToken.None);
            }
        );
    }

    private void ConfigureSerialPort()
    {
        _serialPort = new SerialPort();

        _serialPort.PortName = _serialConfiguration?.SerialPortName ?? "COM4";
        _serialPort.BaudRate = _serialConfiguration?.BaudRate ?? 9600;
        _serialPort.DtrEnable = true;
        _serialPort.RtsEnable = true;
        _serialPort.WriteTimeout = _serialConfiguration?.WriteTimeout ?? 1000;
        _serialPort.ReadTimeout = _serialConfiguration?.ReadTimeout ?? 1000;

        _serialPort.Open();
    }

    private async Task<int> SendCommandToExternalDevice(
        string command,
        object[] parameters)
    {
        var buffer = new BufferBlock<byte[]>();
        var consumerTask = ConsumeCommandAsync(buffer);

        ProduceCommand(buffer, command, parameters);

        return await consumerTask;
    }

    private byte[] PrepareCommand(
        string command,
        object[] parameters)
    {
        return SerialCommandUtils.ComposeRequest(command, parameters);
    }

    private void ProduceCommand(
        ITargetBlock<byte[]> target,
        string command,
        object[] parameters)
    {
        var data = PrepareCommand(command, parameters);

        target.Post(data);
        target.Complete();
    }

    private async Task<int> ConsumeCommandAsync(ISourceBlock<byte[]> source)
    {
        try
        {
            await _commandInProgressSem.WaitAsync();
            if (_commandInProgress)
            {
                _commandInProgressSem.Release();
                SmartFarmerLog.Warning("Another operation in progress");
                return -1;
            }

            _commandInProgress = true;
            _commandInProgressSem.Release();

            while (await source.OutputAvailableAsync())
            {
                byte[] data = await source.ReceiveAsync();
                var length = data.Length;

                if (!_serialPort.IsOpen)
                {
                    SmartFarmerLog.Error($"Serial port {_serialPort.PortName} is closed");
                    return -1;
                }

                SerialCommandUtils.ParseRequest(
                    data, 
                    out var requestId, 
                    out var command, 
                    out var parameters);

                SmartFarmerLog.Debug($"sending request {command}\tID: {requestId}");

                _serialPort.Write(data, 0, length);

                await Task.Delay(200);

                int result = -1;
                // read outcome
                string receivedValue = null;
                
                var attempts = _maxReadAttempts;
                Stopwatch sw = new Stopwatch();

                sw.Start();
                while (attempts > 0)
                {
                    try
                    {
                        receivedValue = _serialPort.ReadLine();
                    }
                    catch (TimeoutException)
                    {
                    }
                    catch (Exception ex)
                    {
                        SmartFarmerLog.Exception(ex);
                        
                        throw new FarmerTaskExecutionException(
                            null,
                            null,
                            "serial port communication error",
                            ex,
                            SmartFarmer.Alerts.AlertCode.SerialCommunicationException,
                            SmartFarmer.Alerts.AlertLevel.Error,
                            SmartFarmer.Alerts.AlertSeverity.Medium);
                    }

                    if (SerialCommandUtils.IsRequestFinalResult(receivedValue))
                    {
                        break;
                    }

                    if (SerialCommandUtils.IsRequestUpdateResult(receivedValue))
                    {
                        ProcessRequestUpdateResult(requestId, command, receivedValue);
                        continue;
                    }

                    await Task.Delay(_delayBetweenReadAttempts);
                    SmartFarmerLog.Debug($"attempt {_maxReadAttempts - attempts + 1}/{_maxReadAttempts}");
                    attempts--;
                };
                sw.Stop();

                if (attempts <= 0)
                {
                    SmartFarmerLog.Error($"No valid data received for {sw.Elapsed.TotalSeconds} seconds");
                    return result;
                }

                SmartFarmerLog.Debug($"received message from serial port:\n\t->{receivedValue.Replace("\r\n", "").Replace("\n", "")}");
                result = GetReceivedValueOutcome(requestId, receivedValue);

                if (result == -1)
                {
                    SmartFarmerLog.Error($"invalid response for command: \"{command}\". Received {receivedValue}");
                }

                return result;
            }
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return -1;
        }
        finally
        {
            await _commandInProgressSem.WaitAsync();
            _commandInProgress = false;
            _commandInProgressSem.Release();
        }

        return 0;
    }

    private void ProcessRequestUpdateResult(string expectedRequestId, string command, string receivedValue)
    {
        SerialCommandUtils.ParsePartialResponse(
            receivedValue, 
            out var requestId, 
            out var resultStr);
        
        
        if (requestId != expectedRequestId)
        {
            return;
        }

        NotifyPartialResultByCommand(command, resultStr);
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
