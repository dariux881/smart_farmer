using System;
using SmartFarmer.Misc;

namespace SmartFarmer.Movement;

public class FarmerDevicePositionInTime : IFarmer5dPoint
{
    public string RunId { get; set; }
    public DateTime PositionDt { get; set; }

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Alpha { get; set; }
    public double Beta { get; set; }

    public override string ToString()
    {
        return $"{PositionDt.ToString("G")}) {X} - {Y} - {Z} - {Alpha} - {Beta}";
    }
}
