using System;
using SmartFarmer.Settings;

public class DefaultFarmerSettings : IFarmerSettings
{
    public bool AUTOIRRIGATION_AUTOSTART => GlobalSettings.DEFAULT_AUTOIRRIGATION_AUTOSTART;
    public FarmerPoint TOOLS_COLLECTOR_POSITION => GlobalSettings.TOOLS_COLLECTOR_POSITION;
    public DateTime AUTOIRRIGATION_PLANNED_TIME => GlobalSettings.DEFAULT_AUTOIRRIGATION_PLANNED_TIME;
}