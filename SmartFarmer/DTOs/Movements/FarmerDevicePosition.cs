using System;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using SmartFarmer.DTOs.Security;
using SmartFarmer.Movement;

namespace SmartFarmer.DTOs.Movements;

public class FarmerDevicePosition : FarmerDevicePositionInTime
{
    public FarmerDevicePosition()
    {
        
    }
    
    public string ID { get; set; }
    [JsonIgnore]
    public FarmerGarden Garden { get; set; }
    public string GardenId { get; set; }
    [JsonIgnore]
    public User User { get; set; }
    public string UserId { get; set; }

    public override string ToString()
    {
        return $"{X}, {Y}, {Z}, {Alpha}, {Beta}";
    }
}