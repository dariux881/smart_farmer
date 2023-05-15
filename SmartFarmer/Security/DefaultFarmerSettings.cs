using System;
using SmartFarmer.Position;
using SmartFarmer.Settings;

public class DefaultFarmerSettings : IFarmerSettings
{
    public bool AUTOIRRIGATION_AUTOSTART => GlobalSettings.DEFAULT_AUTOIRRIGATION_AUTOSTART;
    public Farmer2dPoint TOOLS_COLLECTOR_POSITION => GlobalSettings.TOOLS_COLLECTOR_POSITION;
    public string AUTOIRRIGATION_PLANNED_CRONSCHEDULE => GlobalSettings.AUTOIRRIGATION_PLANNED_CRONSCHEDULE;
}