using System;
using SmartFarmer.DTOs.Movements;

namespace SmartFarmer.Services;

public class DevicePositionEventArgs : EventArgs
{
    public FarmerDevicePosition Position { get; }

    public DevicePositionEventArgs(FarmerDevicePosition position)
    {
        Position = position;
    }
}