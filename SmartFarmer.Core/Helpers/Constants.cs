namespace SmartFarmer.Helpers;

public static class Constants
{
    public const string HEADER_AUTHENTICATION_TOKEN = "Token";

    /*** GROUND HANDLING ***/
    public const double METERS_IN_CELL = 0.2;

    /*** AUTHORIZATIONS ***/
    public const string AUTH_READ_GROUND = "readGround";
    public const string AUTH_EDIT_GROUND = "editGround";
    public const string AUTH_READ_USERS = "readUsers";
    public const string AUTH_EDIT_USERS = "editUsers";

    /*** SERIAL COMM CONSTANTS ***/
    public const string COMMAND_PARAM_SEPARATOR = "#";
    public const string COMMAND_REQUEST_SEPARATOR = "!";

}