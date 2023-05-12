namespace SmartFarmer.Tasks;

public interface IFarmerCliCommand
{
    string UserId { get; }
    string GroundId { get; }
    string Command { get; }
    FarmerCliCommandArgs Args { get; }
}