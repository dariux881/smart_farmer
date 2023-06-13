using System;

namespace SmartFarmer.Tasks;

public class FarmerPlanRequestData
{
    public string PlanName { get; set; }
    public string GroundId { get; set; }
    public int Priority { get; set; }
    public DateTime? ValidFromDt { get; set; }
    public DateTime? ValidToDt { get; set; }
    public string CronSchedule { get; set; }
    public FarmerPlanStepRequestData[] Steps { get; set; }
}