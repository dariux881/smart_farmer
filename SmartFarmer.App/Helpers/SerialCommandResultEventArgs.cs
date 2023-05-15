using System;

namespace SmartFarmer.Handlers;

public class SerialCommandPartialResultEventArgs : EventArgs
{
    public string ExpectedRequestId { get; }
    public string RequestId { get; }
    public string Command { get; }
    public string Result { get; }

    public SerialCommandPartialResultEventArgs(string expectedRequestId, string requestId, string command, string result)
    {
        ExpectedRequestId = expectedRequestId;
        RequestId = requestId;
        Command = command;
        Result = result;
    }
}