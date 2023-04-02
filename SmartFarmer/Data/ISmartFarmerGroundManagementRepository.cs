using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.Plants;

namespace SmartFarmer.Data;

public interface ISmartFarmerGroundManagementRepository
{
    Task<IEnumerable<IFarmerGround>> GetFarmerGroundByUserIdAsync(string userId);
    Task<IFarmerGround> GetFarmerGroundByIdForUserAsync(string userId, string groundId);

    Task<IFarmerPlantInstance> GetFarmerPlantInstanceById(string id, string userId = null);
    Task<IEnumerable<IFarmerPlantInstance>> GetFarmerPlantsInstanceById(string[] ids, string userId = null);
    Task<IrrigationHistory> GetFarmerIrrigationHistoryByPlant(string plantId, string userId = null);
    Task<bool> MarkIrrigationInstance(FarmerPlantIrrigationInstance irrigation, string userId = null);

    Task<IFarmerPlant> GetFarmerPlantById(string id);
    Task<IEnumerable<IFarmerPlant>> GetFarmerPlantsById(string[] ids);

    Task<IFarmerGround> CreateFarmerGround(string userId, FarmerGroundRequestData data);
    Task<bool> AddFarmerPlantInstance(string userId, FarmerPlantRequestData data);
}
