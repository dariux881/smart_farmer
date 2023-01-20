using System;

namespace SmartFarmer.Settings
{
    public class GlobalSettings
    {
        public const int DEFAULT_CELL_SIZE_IN_CM = 10;
        public const bool DEFAULT_AUTOIRRIGATION_AUTOSTART = true;
        public static readonly DateTime DEFAULT_AUTOIRRIGATION_PLANNED_TIME = 
            new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                DateTime.UtcNow.Day,
                2, 0, 0);
    }
}