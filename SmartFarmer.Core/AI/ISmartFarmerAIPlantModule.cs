using System.Threading.Tasks;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;

namespace SmartFarmer.AI;

public interface ISmartFarmerAIPlantModule : ISmartFarmerAIModule<IFarmerPlantInstance>
{
    string PlantId { get; }
    string PlantBotanicalName { get; }
}