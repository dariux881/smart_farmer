using System;
using System.Collections.Generic;

namespace SmartFarmer.Tasks;

public class FarmerPlanExecutionResult
{
    public FarmerPlanExecutionResult()
    {
        TaskResults = new Dictionary<string, object>();
    }

    public string PlanId { get; set; }
    public bool IsSuccess { get; set; }
    public Exception LastException { get; set; }
    public Dictionary<string, object> TaskResults { get; set; }
}
