using System.Collections.Concurrent;
using SmartFarmer.Handlers;
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

    public IFarmerDeviceManager GetDeviceManager(string groundId)
    {

        var key = groundId;
        if (_managers.TryGetValue(key, out var manager))
        {
            return manager;
        }

        var newManager = _managerFactory.GetNewDeviceManager(groundId);
        _managers.TryAdd(key, newManager);

        return newManager;
    }
}
