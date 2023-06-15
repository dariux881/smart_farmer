using System;
using System.Collections.Generic;

namespace SmartFarmer.Tasks;

public class FarmerPlanStepRequestData
{
    public string TaskClassFullName { get; set; }
    public string TaskInterfaceFullName { get; set; }
    public TimeSpan Delay { get; set; }
    public IDictionary<string, string> BuildParameters { get; set; }
}