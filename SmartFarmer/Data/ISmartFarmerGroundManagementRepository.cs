using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Plants;

namespace SmartFarmer.Data;

public interface ISmartFarmerGroundManagementRepository
{
    Task<IEnumerable<IFarmerGround>> GetFarmerGroundByUserIdAsync(string userId);
    Task<IFarmerGround> GetFarmerGroundByIdForUserAsync(string userId, string groundId);

    Task<IFarmerPlantInstance> GetPlantById(string id);

}
