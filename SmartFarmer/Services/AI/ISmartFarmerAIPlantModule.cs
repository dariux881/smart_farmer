using System.Threading.Tasks;
using SmartFarmer.Tasks;

namespace SmartFarmer.Services.AI;

public interface ISmartFarmerAIPlantModule
{
    string PlantKindId { get; }
    Task<IFarmerHoverPlan> GenerateHoverPlan(string plantKindId);
    Task<bool> Execute(FarmerHoverPlanResult planResult);
}