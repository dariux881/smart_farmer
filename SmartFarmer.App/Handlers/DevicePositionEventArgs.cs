using System;
using SmartFarmer.Misc;
using SmartFarmer.Movement;

namespace SmartFarmer.Handlers;

public class DevicePositionEventArgs : EventArgs
{
    public FarmerDevicePositionInTime Position { get; }

    public DevicePositionEventArgs(string positionStr)
    {
        Position = positionStr.Deserialize<FarmerDevicePositionInTime>();
    }
}
