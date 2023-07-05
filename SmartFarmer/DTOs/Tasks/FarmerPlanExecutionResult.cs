using System.Collections.Generic;
using SmartFarmer.Tasks;

namespace SmartFarmer.DTOs.Tasks;

public class FarmerPlanExecutionResult : IFarmerPlanExecutionResult
{
    public string PlanId { get; set; }

    public bool IsSuccess { get; set; }

    public string ErrorMessage { get; set; }

    public List<FarmerStepExecutionResult> StepResults { get; set; }
}