using System;

namespace SmartFarmer.Handlers;

public class CliCommandResultEventArgs : EventArgs
{
    public string Result { get; }
    
    public CliCommandResultEventArgs(string result)
    {
        Result = result;
    }
}