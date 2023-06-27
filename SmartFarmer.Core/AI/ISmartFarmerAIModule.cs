using System.Threading.Tasks;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.AI;

public interface ISmartFarmerAIModule<T>
{
    Task<IFarmerPlan> GenerateHoverPlan(T planTarget);
    Task<FarmerAIDetectionLog> ExecuteDetection(FarmerPlanExecutionResult planResult);
}
