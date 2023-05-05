namespace SmartFarmer.Helpers;

public class ExternalDeviceProtocolConstants
{
    public const string END_COMMAND = "END";
    public const string UPDATE_COMMAND = "UPD";

    public const string STOP = "STP";
    public const string SLEEP = "SLP";
    public const string MOVE_XY_COMMAND = "MXY";
    public const string MOVE_TO_HEIGHT_COMMAND = "MHT";
    public const string MOVE_TO_MAX_HEIGHT_COMMAND = "MMH";
    public const string TURN_HORIZONTAL_COMMAND = "THZ";
    public const string TURN_VERTICAL_COMMAND = "TVT";
    public const string HANDLE_PUMP_COMMAND = "PMP";

    public const string GET_HUMIDITY_LEVEL = "HUL";

    public const int    INVALID_PARAMETERS = -1;
}