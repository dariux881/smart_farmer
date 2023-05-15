using System;
using SmartFarmer.Position;

namespace SmartFarmer.Settings;

public class GlobalSettings
{
    public const int DEFAULT_CELL_SIZE_IN_CM = 10;
    public const bool DEFAULT_AUTOIRRIGATION_AUTOSTART = true;
    public static readonly string AUTOIRRIGATION_PLANNED_CRONSCHEDULE = "0 0 6 ? * * *"; // every day at 6 a.m.
    
    public static readonly Farmer2dPoint TOOLS_COLLECTOR_POSITION = 
        new Farmer2dPoint(0, 0);
}