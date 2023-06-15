using System.Collections.Generic;

namespace SmartFarmer.Movement;

public class FarmerDevicePositionsRequestData
{
    public string GardenId { get; set; }
    public List<FarmerDevicePositionInTime> Positions { get; set; }
}