using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SmartFarmer.Exceptions;
using SmartFarmer.Configurations;
using SmartFarmer.Misc;

namespace SmartFarmer.Handlers;

public class FarmerGroundSerialHandler : IDisposable
{
    private SerialCommunicationConfiguration _serialConfiguration;
    private SerialPort _serialPort;
    private int _delayBetweenReadAttempts;
    private int _maxReadAttempts;
    private SemaphoreSlim _commandInProgressSem;
    private bool _commandInProgress;

    public FarmerGroundSerialHandler(SerialCommunicationConfiguration serialConfiguration)
    {
        _serialConfiguration = serialConfiguration;

        _delayBetweenReadAttempts = _serialConfiguration?.DelayBetweenReadAttempts ?? 2000;
        _maxReadAttempts = _serialConfiguration?.MaxReadAttempts ?? 10;

        _commandInProgressSem = new SemaphoreSlim(1);

        ConfigureSerialPort();
    }

    public event EventHandler<SerialCommandPartialResultEventArgs> PartialResultReceived;

    public void Dispose()
    {
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

    public async Task<SerialCommandFinalResult> SendCommandToExternalDevice(
        string command,
        object[] parameters)
    {
        var buffer = new BufferBlock<byte[]>();
        var consumerTask = ConsumeCommandAsync(buffer);

        ProduceCommand(buffer, command, parameters);

        return await consumerTask;
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

    private async Task<SerialCommandFinalResult> ConsumeCommandAsync(ISourceBlock<byte[]> source)
    {
        try
        {
            await _commandInProgressSem.WaitAsync();
            if (_commandInProgress)
            {
                _commandInProgressSem.Release();
                SmartFarmerLog.Warning("Another operation in progress");
                return BuildFailingResult();
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
                    return BuildFailingResult();
                }

                SerialCommandUtils.ParseRequest(
                    data,
                    out var requestId,
                    out var command,
                    out var parameters);

                SmartFarmerLog.Debug($"sending request {command}\tID: {requestId}");

                _serialPort.Write(data, 0, length);

                await Task.Delay(200);

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
                    return BuildFailingResult(requestId, null, command, null, null);
                }

                return ProcessFinalResult(requestId, command, receivedValue);
            }
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return BuildFailingResult(exception: ex);
        }
        finally
        {
            await _commandInProgressSem.WaitAsync();
            _commandInProgress = false;
            _commandInProgressSem.Release();
        }

        return ProcessFinalResult(null, null, null);
    }

    private SerialCommandFinalResult ProcessFinalResult(string expectedRequestId, string command, string receivedValue)
    {
        SerialCommandUtils.ParsePartialResponse(
            receivedValue, 
            out var requestId, 
            out var resultStr);
        
        return BuildResult(
            expectedRequestId,
            requestId,
            command,
            resultStr,
            true,
            null);
    }

    private void ProcessRequestUpdateResult(string expectedRequestId, string command, string receivedValue)
    {
        SerialCommandUtils.ParsePartialResponse(
            receivedValue, 
            out var requestId, 
            out var resultStr);

        PartialResultReceived?.Invoke(
            this, 
            new SerialCommandPartialResultEventArgs(
                expectedRequestId,
                requestId,
                command, 
                receivedValue
            ));
    }

    private SerialCommandFinalResult BuildResult(
        string expectedRequestId = null,
        string requestId = null,
        string command = null,
        string resultStr = null,
        bool isSuccess = true,
        Exception exception = null)
    {
        return new SerialCommandFinalResult(
            expectedRequestId, 
            requestId, 
            command, 
            resultStr, 
            isSuccess, 
            exception);
    }

    private SerialCommandFinalResult BuildFailingResult(
        string expectedRequestId = null,
        string requestId = null,
        string command = null,
        string resultStr = null,
        Exception exception = null)
    {
        return BuildResult(
            expectedRequestId,
            requestId,
            command,
            resultStr,
            false,
            exception);
    }
}