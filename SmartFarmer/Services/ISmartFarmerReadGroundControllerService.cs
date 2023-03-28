using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Services;

public interface ISmartFarmerReadGroundControllerService
{
    Task<IEnumerable<IFarmerGround>> GetFarmerGroundByUserIdAsync(string userId);
    Task<IFarmerGround> GetFarmerGroundByIdForUserAsync(string userId, string groundId);

    Task<IFarmerPlantInstance> GetFarmerPlantInstanceByIdForUserAsync(string userId, string plantId);
    Task<IEnumerable<IFarmerPlantInstance>> GetFarmerPlantInstanceByIdsForUserAsync(string userId, string[] plantIds);

    Task<IFarmerPlant> GetFarmerPlantByIdAsync(string plantId);
    Task<IEnumerable<IFarmerPlant>> GetFarmerPlantByIdsAsync(string[] plantIds);
    Task<IrrigationHistory> GetFarmerIrrigationHistoryByPlantAsync(string userId, string plantId);
    
    Task<IFarmerPlan> GetFarmerPlanByIdForUserAsync(string userId, string planId);
    Task<IEnumerable<IFarmerPlan>> GetFarmerPlanByIdsForUserAsync(string userId, string[] planIds);
    Task<IEnumerable<IFarmerPlanStep>> GetFarmerPlanStepByIdsAsync (string[] ids);

    Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByGroundIdAsync(string userId, string groundId);
    Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByIdAsync(string userId, string[] ids);
}
