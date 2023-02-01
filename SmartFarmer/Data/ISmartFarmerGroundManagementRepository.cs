using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Plants;

namespace SmartFarmer.Data;

public interface ISmartFarmerGroundManagementRepository
{
    Task<IEnumerable<IFarmerGround>> GetFarmerGroundByUserIdAsync(string userId);
    Task<IFarmerGround> GetFarmerGroundByIdForUserAsync(string userId, string groundId);

    Task<IFarmerPlantInstance> GetFarmerPlantInstanceById(string id, string userId = null);
    Task<IEnumerable<IFarmerPlantInstance>> GetFarmerPlantsInstanceById(string[] ids, string userId = null);

    Task<IFarmerPlant> GetFarmerPlantById(string id);
    Task<IEnumerable<IFarmerPlant>> GetFarmerPlantsById(string[] ids);
}
