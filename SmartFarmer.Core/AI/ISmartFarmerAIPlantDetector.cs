using SmartFarmer.AI.Base;
using SmartFarmer.Plants;

namespace SmartFarmer.AI;

public interface ISmartFarmerAIPlantDetector : ISmartFarmerAIDetector<IFarmerPlantInstance>
{
    string PlantId { get; }
    string PlantBotanicalName { get; }
}
