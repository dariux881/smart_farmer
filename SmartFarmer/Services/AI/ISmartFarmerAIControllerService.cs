using System.Threading.Tasks;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Services.AI;

public interface ISmartFarmerAIControllerService
{
    Task<IFarmerPlan> GenerateHoverPlan(string userId, string plantInstanceId);
    Task<bool> IsValidHoverPlan(string userId, string planId);
    Task<bool> AnalyseHoverPlanResult(string userId, IFarmerPlanExecutionResult result);
}