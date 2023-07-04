using System.Collections.Generic;

namespace SmartFarmer.Tasks;

public interface IFarmerPlanExecutionResult
{
    string PlanId { get; }
    bool IsSuccess { get; }
    string ErrorMessage { get; }
    Dictionary<string, object> TaskResults { get; set; }
}