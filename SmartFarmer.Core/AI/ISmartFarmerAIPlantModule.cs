using SmartFarmer.Plants;

namespace SmartFarmer.AI;

public interface ISmartFarmerAIPlantModule : ISmartFarmerAIModule<IFarmerPlantInstance>
{
    string PlantId { get; }
    string PlantBotanicalName { get; }
}