using SmartFarmer.Handlers;
using SmartFarmer.Movement;

namespace SmartFarmer.Handlers.Providers;

public interface IFarmerDeviceKindProvider
{
    IFarmerDeviceManager GetDeviceManager(string gardenId);
}