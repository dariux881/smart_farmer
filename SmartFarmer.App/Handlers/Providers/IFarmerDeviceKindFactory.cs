using SmartFarmer.Helpers;
using SmartFarmer.Movement;

namespace SmartFarmer.Handlers.Providers;

public interface IFarmerDeviceKindFactory
{
    IFarmerDeviceManager GetNewDeviceManager(string groundId, DeviceKindEnum kind);
}
