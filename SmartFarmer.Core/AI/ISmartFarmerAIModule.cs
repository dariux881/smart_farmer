using System.Threading.Tasks;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.AI;

public interface ISmartFarmerAIModule
{
    Task<FarmerAIDetectionLog> ExecuteDetection(object stepData);
}

public interface ISmartFarmerAIModule<T> : ISmartFarmerAIModule
{
    Task<IFarmerPlan> GenerateHoverPlan(T planTarget);
}
