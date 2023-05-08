using System;

namespace SmartFarmer.Handlers;

public class SerialCommandFinalResult
{
    public string ExpectedRequestId { get; }
    public string RequestId { get; }
    public string Command { get; }
    public string Result { get; }
    public bool IsSuccess { get; }
    public Exception Exception { get; }

    public SerialCommandFinalResult(
        string expectedRequestId, 
        string requestId, 
        string command, 
        string result, 
        bool isSuccess,
        Exception exception)
    {
        ExpectedRequestId = expectedRequestId;
        RequestId = requestId;
        Command = command;
        Result = result;
        IsSuccess = isSuccess;
    }
}