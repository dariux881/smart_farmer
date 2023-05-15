using SmartFarmer.Helpers;

namespace SmartFarmer.Configurations;

public class GroundConfiguration
{
    public string GroundId { get; set; }
    public SerialCommunicationConfiguration SerialConfiguration { get; set; }
    public DeviceKindEnum DeviceKind { get; set; }
}