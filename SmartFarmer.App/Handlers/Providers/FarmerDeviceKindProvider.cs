using System.Collections.Concurrent;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;

namespace SmartFarmer.Handlers.Providers;

public class FarmerDeviceKindProvider : IFarmerDeviceKindProvider
{
    private ConcurrentDictionary<string, IFarmerDeviceManager> _managers;
    private readonly IFarmerDeviceKindFactory _managerFactory;

    public FarmerDeviceKindProvider()
    {
        _managers = new ConcurrentDictionary<string, IFarmerDeviceManager>();
        _managerFactory = FarmerServiceLocator.GetService<IFarmerDeviceKindFactory>(true);
    }

    public IFarmerDeviceManager GetDeviceManager(string groundId, DeviceKindEnum kind)
    {
        var key = BuildKey(groundId, kind);
        if (_managers.TryGetValue(key, out var manager))
        {
            return manager;
        }

        var newManager = _managerFactory.GetNewDeviceManager(groundId, kind);
        _managers.TryAdd(key, newManager);

        return newManager;
    }

    private string BuildKey(string groundId, DeviceKindEnum kind)
    {
        return kind + groundId;
    }
}
