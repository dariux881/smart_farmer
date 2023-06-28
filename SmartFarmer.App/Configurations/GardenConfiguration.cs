using SmartFarmer.Helpers;

namespace SmartFarmer.Configurations;

public class GardenConfiguration
{
    public string GardenId { get; set; }
    public SerialCommunicationConfiguration SerialConfiguration { get; set; }
    public DeviceKindEnum DeviceKind { get; set; }
    public CameraConfiguration CameraConfiguration { get; set; } 
}