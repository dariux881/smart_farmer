using System;

namespace SmartFarmer.Services;

public class DevicePositionsEventArgs : EventArgs
{
    public string[] PositionIds { get; }

    public DevicePositionsEventArgs(string[] positions)
    {
        PositionIds = positions;
    }
}