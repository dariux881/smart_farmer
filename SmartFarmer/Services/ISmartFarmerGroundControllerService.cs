using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Plants;

namespace SmartFarmer.Services;

public interface ISmartFarmerGroundControllerService
{
    Task<IEnumerable<IFarmerGround>> GetFarmerGroundByUserIdAsync(string userId);
    Task<IFarmerGround> GetFarmerGroundByIdForUserAsync(string userId, string groundId);

    Task<IFarmerPlantInstance> GetFarmerPlantInstanceByIdForUserAsync(string userId, string plantId);
}
