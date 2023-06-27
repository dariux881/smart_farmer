using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.AI;

public interface ISmartFarmerAITaskModule : ISmartFarmerAIModule<IFarmerTask>
{
    string TaskInterfaceFullName { get; }
}