namespace SmartFarmer.Movement;

public class FarmerDevicePositionRequestData
{
    public string GroundId { get; set; }
    public string RunId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Alpha { get; set; }
    public double Beta { get; set; }
}