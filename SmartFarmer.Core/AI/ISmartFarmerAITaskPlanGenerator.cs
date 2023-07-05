using SmartFarmer.AI.Base;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.AI;

public interface ISmartFarmerAITaskPlanGenerator : ISmartFarmerAIPlanGenerator<IFarmerTask>, IHasTaskInterfaceFullName
{
}
