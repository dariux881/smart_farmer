using SmartFarmer.AI;
using SmartFarmer.DTOs.Plants;

namespace SmartFarmer.Services.AI;

public interface ISmartFarmerAIControllerServiceProvider
{
    ISmartFarmerAIPlantPlanGenerator GetAIPlantPlanGenerator(FarmerPlantInstance plant);
    ISmartFarmerAITaskPlanGenerator GetAITaskPlanGenerator(string taskInterfaceFullName);

    ISmartFarmerAIPlantDetector GetAIPlantDetector(FarmerPlantInstance plant);
    ISmartFarmerAITaskDetector GetAITaskDetector(string taskInterfaceFullName);
}