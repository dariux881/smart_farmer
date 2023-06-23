using SmartFarmer.AI;

namespace SmartFarmer.Services.AI;

public interface ISmartFarmerAIControllerServiceProvider
{
    ISmartFarmerAIPlantModule GetAIPlantModuleByPlantId(string plantInstanceId, string plantKindId);
    ISmartFarmerAIPlantModule GetAIPlantModuleByPlantBotanicalName(string botanicalName);
}