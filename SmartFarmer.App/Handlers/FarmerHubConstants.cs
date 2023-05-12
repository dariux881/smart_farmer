namespace SmartFarmer.Handlers;

public class FarmerHubConstants
{
    public const string SUBSCRIBE = "AddToGroupAsync";

    #region "Position"
    
    public const string INSERT_DEVICE_POSITION = "InsertNewPositionAsync";
    public const string NOTIFY_DEVICE_POSITION = "NotifyNewPositionAsync";
    public const string ON_NEW_POSITION_RECEIVED = "NewDevicePosition";
    
    #endregion

    #region "Alert"

    public const string CHANGE_ALERT_STATUS = "SendNewAlertStatusAsync";
    public const string NOTIFY_NEW_ALERT_STATUS = "NotifyNewAlertStatusAsync";
    public const string ON_ALERT_STATUS_CHANGED = "AlertStatusChanged";

    #endregion
}