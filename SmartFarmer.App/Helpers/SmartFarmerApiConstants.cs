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

    #region Garden Management

    public const string GARDENS_BASE = "farmerGarden";
    public const string GET_GARDEN = GARDENS_BASE + "/garden";
    public const string GET_PLANT_IN_GARDEN = GARDENS_BASE + "/plantInGarden";
    public const string GET_PLANTS_IN_GARDEN = GARDENS_BASE + "/plantsInGarden";
    public const string GET_PLANT_IRRIGATION_HISTORY = GARDENS_BASE + "/irrigationHistory";
    public const string SET_PLANT_IRRIGATION_STEP = GARDENS_BASE + "/markIrrigation";
    public const string GET_PLANT = GARDENS_BASE + "/plant";
    public const string GET_PLANTS = GARDENS_BASE + "/plants";
    public const string UPDATE_DEVICE_POSITION = GARDENS_BASE + "/notifyPosition";
    public const string UPDATE_DEVICE_POSITIONS = GARDENS_BASE + "/notifyPositions";

    #endregion // Garden Management

    #region Plans Management

    public const string PLANS_BASE = "farmerPlan";
    public const string GET_PLAN = PLANS_BASE + "/plan";
    public const string GET_PLANS = PLANS_BASE + "/plans";
    public const string GET_PLAN_STEPS = PLANS_BASE + "/steps";
    public const string NOTIFY_PLAN_EXECUTION_RESULT = PLANS_BASE + "/planExecutionResult";

    #endregion // Plans Management

    #region Alerts Management

    public const string ALERTS_BASE = "farmerAlert";
    public const string GET_ALERTS = ALERTS_BASE + "/alerts";
    public const string SET_ALERT_READ = ALERTS_BASE + "/markAlert";
    public const string CREATE_ALERT = ALERTS_BASE + "/createAlert";

    #endregion // Alerts Management

    #region AI

    public const string AI_BASE = "farmerAI";
    public const string GENERATE_PLAN_FOR_PLANT = AI_BASE + "/GetPlanForPlant";

    #endregion
}