namespace SmartFarmer.Services.AI;

public interface ISmartFarmerAIControllerServiceProvider
{
    ISmartFarmerAIPlantModule GetAIPlantModuleByPlantId(string plantKindId);
}