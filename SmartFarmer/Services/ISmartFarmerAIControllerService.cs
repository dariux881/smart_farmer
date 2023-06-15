using System.Threading.Tasks;
using SmartFarmer.Tasks;

namespace SmartFarmer.Services;

public interface ISmartFarmerAIControllerService
{
    Task<IFarmerHoverPlan> GenerateHoverPlan(string userId, string plantInstanceId);
}