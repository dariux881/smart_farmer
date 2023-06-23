using System.Threading.Tasks;
using SmartFarmer.Tasks;

namespace SmartFarmer.AI;

public interface ISmartFarmerAIModule<T>
{
    Task<IFarmerHoverPlan> GenerateHoverPlan(T planTarget);
    Task<FarmerAIDetectionLog> ExecuteDetection(FarmerHoverPlanResult planResult);
}
