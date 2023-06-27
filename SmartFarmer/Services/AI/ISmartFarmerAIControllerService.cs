using System.Threading.Tasks;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Services.AI;

public interface ISmartFarmerAIControllerService
{
    Task<IFarmerPlan> GenerateHoverPlan(string userId, string plantInstanceId);
    bool IsHoverPlan(string userId, string planId);
    Task<bool> AnalyseHoverPlanResult(string userId, FarmerPlanExecutionResult result);
}