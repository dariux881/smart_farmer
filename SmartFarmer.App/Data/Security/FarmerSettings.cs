using System;
//using Newtonsoft.Json;

namespace SmartFarmer.Data.Security;

public class FarmerSettings : IFarmerSettings
{
    public bool AUTOIRRIGATION_AUTOSTART { get; set; }

    public Farmer2dPoint TOOLS_COLLECTOR_POSITION { get; set; }

    public DateTime AUTOIRRIGATION_PLANNED_TIME { get; set; }
}