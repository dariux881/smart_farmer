using System;
using SmartFarmer.Misc;
using SmartFarmer.Tasks;

namespace SmartFarmer.Handlers;

public class NewCliCommandEventArgs : EventArgs
{
    public IFarmerCliCommand Command { get; }
    
    public NewCliCommandEventArgs(string cliCommandStr)
    {
        Command = cliCommandStr.Deserialize<FarmerCliCommand>();
    }
}