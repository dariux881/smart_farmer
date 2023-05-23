using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Data;

public interface ISmartFarmerPlanManagementRepository
{
    Task<IFarmerPlan> GetFarmerPlanByIdAsync(string id, string userId);
    Task<IEnumerable<string>> GetFarmerPlansInGround(string groundId, string userId);
    Task<IEnumerable<IFarmerPlan>> GetFarmerPlanByIdsAsync(string[] ids, string userId);
    Task<IEnumerable<IFarmerPlanStep>> GetFarmerPlanStepByIdsAsync (string[] ids);
    Task<string> SaveFarmerPlan(FarmerPlan plan);
    Task<bool> DeleteFarmerPlan(FarmerPlan plan);
}