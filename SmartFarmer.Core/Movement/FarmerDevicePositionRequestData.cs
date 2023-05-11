using System;
using SmartFarmer.Misc;

namespace SmartFarmer.Movement;

public class FarmerDevicePositionRequestData
{
    public string GroundId { get; set; }
    public string RunId { get; set; }
    public DateTime? PositionDt { get; set; }
    public Farmer5dPoint Position { get; set; }
}