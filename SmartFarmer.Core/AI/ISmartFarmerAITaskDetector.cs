using SmartFarmer.AI.Base;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.AI;

public interface ISmartFarmerAITaskDetector : ISmartFarmerAIDetector<IFarmerTask>, IHasTaskInterfaceFullName
{
}