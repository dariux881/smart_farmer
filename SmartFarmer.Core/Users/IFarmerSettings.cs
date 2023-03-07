using System;

public interface IFarmerSettings
{
    bool AUTOIRRIGATION_AUTOSTART { get; }
    FarmerPoint TOOLS_COLLECTOR_POSITION { get; }
    DateTime AUTOIRRIGATION_PLANNED_TIME { get; }
}