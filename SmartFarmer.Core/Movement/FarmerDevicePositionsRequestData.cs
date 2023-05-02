using System.Collections.Generic;

namespace SmartFarmer.Movement;

public class FarmerDevicePositionsRequestData
{
    public string GroundId { get; set; }
    public string RunId { get; set; }
    public List<FarmerDevicePositionInTime> Positions { get; set; }
}