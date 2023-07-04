using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Services.Plan;

public interface ISmartFarmerPlanControllerService
{
    event EventHandler<PlanEventArgs> NewPlan;
    event EventHandler<PlanEventArgs> PlanDeleted;
    event EventHandler<PlanEventArgs> NewAutoIrrigationPlan;

    Task<IEnumerable<string>> GetFarmerPlanIdsInGardenAsync(string userId, string gardenId);
    Task<IFarmerPlan> GetFarmerPlanByIdForUserAsync(string userId, string planId);
    Task<IEnumerable<IFarmerPlan>> GetFarmerPlanByIdsForUserAsync(string userId, string[] planIds);
    Task<IEnumerable<IFarmerPlanStep>> GetFarmerPlanStepByIdsAsync (string[] ids);

    Task<string> AddPlan(string userId, FarmerPlanRequestData planRequestData);
    Task<bool> DeletePlan(string userId, string planId);
    Task<string> BuildIrrigationPlan(string userId, string gardenId);
    Task AnalysePlanResult(string userId, IFarmerPlanExecutionResult result);

}
