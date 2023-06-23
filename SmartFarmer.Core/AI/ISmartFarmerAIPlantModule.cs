using System.Threading.Tasks;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;

namespace SmartFarmer.AI;

public interface ISmartFarmerAIPlantModule
{
    string PlantId { get; }
    string PlantBotanicalName { get; }
    Task<IFarmerHoverPlan> GenerateHoverPlan(IFarmerPlantInstance plant);
    Task<bool> StartDetection(FarmerHoverPlanResult planResult);
}