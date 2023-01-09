namespace SmartFarmer.Settings
{
    public static class UserDefinedSettingsProvider
    {
        public static UserDefinedSettings GetUserDefinedSettings(string userId)
        {
            //FIXME load by DB
            return new UserDefinedSettings();
        }
    }
}