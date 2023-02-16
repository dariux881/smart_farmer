using System.Threading.Tasks;
using SmartFarmer.DTOs.Plants;

namespace SmartFarmer.Services;

public interface ISmartFarmerEditGroundControllerService
{
    Task<bool> AddFarmerPlantInstance(string userId, FarmerPlantRequestData data);
}
