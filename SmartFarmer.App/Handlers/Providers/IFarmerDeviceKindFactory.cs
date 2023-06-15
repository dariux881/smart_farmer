using SmartFarmer.Handlers;
using SmartFarmer.Movement;

namespace SmartFarmer.Handlers.Providers;

public interface IFarmerDeviceKindFactory
{
    IFarmerDeviceManager GetNewDeviceManager(string gardenId);
}
