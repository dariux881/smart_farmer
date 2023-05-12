using System.Collections.Generic;

namespace SmartFarmer.Tasks;

public class FarmerCliCommand : IFarmerCliCommand
{
    public string UserId { get; set; }
    public string GroundId { get; set; }
    public string Command { get; set; }
    public FarmerCliCommandArgs Args { get; set; }
}