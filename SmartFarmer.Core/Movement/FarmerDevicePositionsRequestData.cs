namespace SmartFarmer.Movement;

public class FarmerDevicePositionsRequestData
{
    public string GroundId { get; set; }
    public string RunId { get; set; }
    public FarmerDevicePositionInTime[] Positions { get; set; }
}