namespace SmartFarmer.Handlers;

public class FarmerHubConstants
{
    public const string SUBSCRIBE = "AddToGroupAsync";

    #region "Position"
    
    public const string INSERT_DEVICE_POSITION = "InsertNewPositionAsync";
    public const string SEND_DEVICE_POSITION_NOTIFICATION = "NotifyNewPositionAsync";
    public const string RECEIVE_DEVICE_POSITION = "NewDevicePosition";
    
    #endregion

    #region "Alert"

    public const string SEND_NEW_ALERT_STATUS = "SendNewAlertStatusAsync";
    public const string SEND_NEW_ALERT_STATUS_NOTIFICATION = "NotifyNewAlertStatusAsync";
    public const string RECEIVE_ALERT_STATUS_CHANGE = "AlertStatusChanged";

    #endregion

    #region "Cli"

    public const string SEND_CLI_COMMAND = "SendCliCommandAsync";
    public const string RECEIVE_CLI_COMMAND = "NewCliCommand";
    public const string SEND_CLI_COMMAND_RESULT = "NotifyCliCommandResult";
    public const string RECEIVE_CLI_COMMAND_RESULT = "ReceiveCliCommandResult";

    #endregion
}