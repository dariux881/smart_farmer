using System.Threading.Tasks;
using SmartFarmer.Tasks;

namespace SmartFarmer.Services.AI;

public interface ISmartFarmerAIControllerService
{
    Task<IFarmerHoverPlan> GenerateHoverPlan(string userId, string plantInstanceId);
    Task<bool> AnalyseHoverPlanResult(string userId, FarmerHoverPlanResult result);
}