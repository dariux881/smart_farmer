using System;
using System.Collections.Generic;
using SmartFarmer.Tasks;

namespace SmartFarmer.Data.Tasks;

public class FarmerPlanExecutionResult : IFarmerPlanExecutionResult
{
    public FarmerPlanExecutionResult()
    {
        StepResults = new List<FarmerStepExecutionResult>();
    }

    public string PlanId { get; set; }

    public bool IsSuccess => LastException == null;

    public string ErrorMessage => LastException?.InnerException?.Message ?? LastException?.Message;

    public Exception LastException { private get; set; }

    public List<FarmerStepExecutionResult> StepResults { get; }
}