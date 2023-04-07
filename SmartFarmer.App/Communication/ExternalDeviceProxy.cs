
using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private Farmer5dPositionNotifier _positionNotifier;
    private SerialCommunicationConfiguration _serialConfiguration;
    private SerialPort _serialPort;

    public ExternalDeviceProxy(SerialCommunicationConfiguration appConfiguration)
    {
        _positionNotifier = new Farmer5dPositionNotifier();
        _positionNotifier.NewPoint += NewPointReceived;

        _serialConfiguration = appConfiguration;

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
            new object[] { heightInCm }) == 1;
        
        if (result)
        {
            _positionNotifier.Z = heightInCm;
        }

        return result;
    }

    public async Task<bool> MoveArmAtMaxHeightAsync(CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.MOVE_TO_MAX_HEIGHT_COMMAND,
            null);
        
        var outcome = result > 0;
        if (outcome)
        {
            _positionNotifier.Z = result;
        }

        return outcome;
    }

    public async Task<bool> MoveOnGridAsync(double x, double y, CancellationToken token)
    {
        var result = await SendCommandToExternalDevice(
            ExternalDeviceProtocolConstants.MOVE_XY_COMMAND,
            new object[] { x, y }) == 1;
        
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
            new object[] { degrees }) == 1;
        
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
            new object[] { degrees }) == 1;
        
        if (result)
        {
            _positionNotifier.Alpha = degrees;
        }

        return result;
    }

    public void Dispose()
    {
        _positionNotifier.NewPoint -= NewPointReceived;
    }

    private void NewPointReceived(object sender, EventArgs args)
    {
        if (sender is IFarmerPointNotifier notifier)
        {
            NewPoint.Invoke(this, args);
        }
    }

    private void ConfigureSerialPort()
    {
        _serialPort = new SerialPort();

        _serialPort.PortName = _serialConfiguration?.SerialPortName ?? "COM4";
        _serialPort.BaudRate = _serialConfiguration?.BaudRate ?? 9600;
        _serialPort.WriteTimeout = _serialConfiguration?.WriteTimeout ?? 1000;
        _serialPort.ReadTimeout = _serialConfiguration?.ReadTimeout ?? 1000;

        _serialPort.Open();
    }

    private async Task<int> SendCommandToExternalDevice(
        string command,
        object[] parameters,
        CancellationToken token)
    {
        const string separator = "#";
        var paramsToSend = 
            parameters == null ? 
                "" :
                parameters.Any() ? 
                    parameters.Aggregate((p1, p2) => p1 + separator + p2) :
                    "";

        var task = await Task.Run(() => {
            try
            {
                _serialPort.WriteLine(command + separator + paramsToSend);

                while (!token.IsCancellationRequested)
                {
                    string a = _serialPort.ReadExisting();
                    Console.WriteLine(a);
                    Thread.Sleep(200);
                }

                string a = _serialPort.ReadExisting();
                Console.WriteLine(a);
                Thread.Sleep(200);
            }
            catch(Exception ex)
            {
                SmartFarmerLog.Exception(ex);
                throw;
            }
        });

    }
}
