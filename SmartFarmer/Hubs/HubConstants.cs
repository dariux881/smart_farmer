namespace SmartFarmer.Hubs;

public class HubConstants
{
    public const string NewPlantInGround = "NewPlantInGround";
    public const string NewPlan = "NewPlan";
    public const string DeletedPlan = "DeletedPlan";
    public const string NewAutoIrrigationPlan = "NewAutoIrrigationPlan";
    public const string NewAlert = "NewAlert";
    public const string AlertStatusChanged = "AlertStatusChanged";
    public const string NewPositionReceivedMessage = "NewDevicePosition";
    public const string NewCommand = "NewCommand";
    public const string NewToolCollectorPosition = "NewToolCollectorPosition";
    public const string DeviceHealthCheckResult = "DeviceHealthCheckResult";
    public const string NewUserSettings = "NewUserSettings";
    public const string NewCliCommand = "NewCliCommand";
    public const string ReceiveCliCommandResult = "ReceiveCliCommandResult";
}