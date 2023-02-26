using System.Threading.Tasks;
using SmartFarmer.DTOs.Plants;

namespace SmartFarmer.Services;

public interface ISmartFarmerEditGroundControllerService
{
    Task<IFarmerGround> CreateFarmerGround(string userId, FarmerGroundRequestData data);
    Task<bool> AddFarmerPlantInstance(string userId, FarmerPlantRequestData data);
}
