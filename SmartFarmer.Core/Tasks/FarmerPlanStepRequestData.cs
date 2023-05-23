using System;

namespace SmartFarmer.Tasks;

public class FarmerPlanStepRequestData
{
    public string TaskClassFullName { get; set; }
    public string TaskInterfaceFullName { get; set; }
    public TimeSpan Delay { get; set; }
    public string[] BuildParameters { get; set; }
}