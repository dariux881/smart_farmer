using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.AI.Base;

public interface ISmartFarmerAIPlanGenerator<T> : ISmartFarmerAIModule
{
    Task<IFarmerPlan> GenerateHoverPlan(T planTarget, string gardenId);
}
