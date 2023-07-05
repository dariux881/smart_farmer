using SmartFarmer.AI.Base;
using SmartFarmer.Plants;

namespace SmartFarmer.AI;

public interface ISmartFarmerAIPlantPlanGenerator : ISmartFarmerAIPlanGenerator<IFarmerPlantInstance>
{
    string PlantId { get; }
    string PlantBotanicalName { get; }
}