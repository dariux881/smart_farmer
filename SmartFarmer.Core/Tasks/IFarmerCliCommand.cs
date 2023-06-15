namespace SmartFarmer.Tasks;

public interface IFarmerCliCommand
{
    string UserId { get; }
    string GardenId { get; }
    string Command { get; }
    FarmerCliCommandArgs Args { get; }
}