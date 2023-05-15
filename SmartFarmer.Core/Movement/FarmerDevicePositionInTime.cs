using System;
using SmartFarmer.Position;

namespace SmartFarmer.Movement;

public class FarmerDevicePositionInTime : Farmer5dPoint
{
    public string RunId { get; set; }
    public DateTime PositionDt { get; set; }

    public FarmerDevicePositionInTime()
    {
        
    }

    public FarmerDevicePositionInTime(Farmer5dPoint position)
        : base(position)
    {
        
    }

    public override string ToString()
    {
        return $"{PositionDt.ToString("G")}) {X} - {Y} - {Z} - {Alpha} - {Beta}";
    }
}
