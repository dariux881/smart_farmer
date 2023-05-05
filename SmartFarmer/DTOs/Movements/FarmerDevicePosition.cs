using System;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using SmartFarmer.DTOs.Security;

namespace SmartFarmer.DTOs.Movements;

public class FarmerDevicePosition
{
    public string ID { get; set; }
    public DateTime PositionDt { get; set; }
    [JsonIgnore]
    public FarmerGround Ground { get; set; }
    public string GroundId { get; set; }
    [JsonIgnore]
    public User User { get; set; }
    public string UserId { get; set; }
    public string RunId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Alpha { get; set; }
    public double Beta { get; set; }

    public override string ToString()
    {
        return $"{X}, {Y}, {Z}, {Alpha}, {Beta}";
    }
}