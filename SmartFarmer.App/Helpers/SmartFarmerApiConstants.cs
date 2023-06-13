namespace SmartFarmer.Helpers;

public class SmartFarmerApiConstants
{
    #region User Management
    
    public const string USER_BASE = "Authentication";
    public const string USER_AUTHENTICATION_HEADER_KEY = "Authorization";
    public const string USER_LOGIN_API = USER_BASE + "/logIn";
    public const string USER_LOGOUT_API = USER_BASE + "/logOut";
    public const string GET_USER_SETTINGS_API = USER_BASE + "/getUserSettings";
    public const string SET_USER_SETTINGS_API = USER_BASE + "/SaveUserSettings";

    #endregion // User Management

    #region Ground Management

    public const string GROUNDS_BASE = "farmerGround";
    public const string GET_GROUND = GROUNDS_BASE + "/ground";
    public const string GET_PLANT_IN_GROUND = GROUNDS_BASE + "/plantInGround";
    public const string GET_PLANTS_IN_GROUND = GROUNDS_BASE + "/plantsInGround";
    public const string GET_PLANT_IRRIGATION_HISTORY = GROUNDS_BASE + "/irrigationHistory";
    public const string SET_PLANT_IRRIGATION_STEP = GROUNDS_BASE + "/markIrrigation";
    public const string GET_PLANT = GROUNDS_BASE + "/plant";
    public const string GET_PLANTS = GROUNDS_BASE + "/plants";
    public const string UPDATE_DEVICE_POSITION = GROUNDS_BASE + "/notifyPosition";
    public const string UPDATE_DEVICE_POSITIONS = GROUNDS_BASE + "/notifyPositions";

    #endregion // Ground Management

    #region Plans Management

    public const string PLANS_BASE = "farmerPlan";
    public const string GET_PLAN = PLANS_BASE + "/plan";
    public const string GET_PLANS = PLANS_BASE + "/plans";
    public const string GET_PLAN_STEPS = PLANS_BASE + "/steps";

    #endregion // Plans Management

    #region Alerts Management

    public const string ALERTS_BASE = "farmerAlert";
    public const string GET_ALERTS = ALERTS_BASE + "/alerts";
    public const string SET_ALERT_READ = ALERTS_BASE + "/markAlert";
    public const string CREATE_ALERT = ALERTS_BASE + "/createAlert";

    #endregion // Alerts Management

}