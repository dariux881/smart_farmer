using System;
//using Newtonsoft.Json;
using SmartFarmer.Position;

namespace SmartFarmer.Data.Security;

public class FarmerSettings : IFarmerSettings
{
    public bool AUTOIRRIGATION_AUTOSTART { get; set; }

    public Farmer2dPoint TOOLS_COLLECTOR_POSITION { get; set; }

    public string AUTOIRRIGATION_PLANNED_CRONSCHEDULE { get; set; }
}