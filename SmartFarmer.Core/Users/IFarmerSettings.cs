using System;

public interface IFarmerSettings
{
    bool AUTOIRRIGATION_AUTOSTART { get; }
    Farmer2dPoint TOOLS_COLLECTOR_POSITION { get; }
    string AUTOIRRIGATION_PLANNED_CRONSCHEDULE { get; }
}