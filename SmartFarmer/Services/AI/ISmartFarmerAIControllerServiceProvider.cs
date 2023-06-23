using SmartFarmer.AI;
using SmartFarmer.DTOs.Plants;

namespace SmartFarmer.Services.AI;

public interface ISmartFarmerAIControllerServiceProvider
{
    ISmartFarmerAIPlantModule GetAIPlantModuleByPlant(FarmerPlantInstance plant);
}