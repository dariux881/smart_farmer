using SmartFarmer.Helpers;
using SmartFarmer.Movement;

namespace SmartFarmer.Handlers.Providers;

public interface IFarmerDeviceKindProvider
{
    IFarmerDeviceManager GetDeviceManager(string groundId, DeviceKindEnum kind);
}