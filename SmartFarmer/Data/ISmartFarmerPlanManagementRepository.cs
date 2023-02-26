using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Data;

public interface ISmartFarmerPlanManagementRepository
{
    Task<IFarmerPlan> GetFarmerPlanByIdAsync(string id, string userId);
    Task<IEnumerable<IFarmerPlan>> GetFarmerPlanByIdsAsync(string[] ids, string userId);
    Task<IEnumerable<IFarmerPlanStep>> GetFarmerPlanStepByIdsAsync (string[] ids);
}