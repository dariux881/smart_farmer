using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartFarmer.Data;

public interface ISmartFarmerGroundControllerService
{
    Task<IEnumerable<IFarmerGround>> GetFarmerGroundByUserIdAsync(string userId);
    Task<IFarmerGround> GetFarmerGroundByIdAsync(string groundId);
}
