using System.Collections.Concurrent;
using System.Threading.Tasks;
using SmartFarmer.Misc;

namespace SmartFarmer.Utils;

public abstract class FarmerServiceLocalProviderBase<T> 
    where T : IFarmerService
{
    private ConcurrentDictionary<string, T> _serviceInstances;
    private string _servicePrefix;

    public FarmerServiceLocalProviderBase(string servicePrefix)
    {
        _serviceInstances = new ConcurrentDictionary<string, T>();

        _servicePrefix = servicePrefix;
    }

    public async Task<string> AddFarmerService(T service)
    {
        var result = _serviceInstances.TryAdd(service.ID, service);

        await Task.CompletedTask;
        return result ? service.ID : null;
    }

    
    public async Task<T> GetFarmerService(string serviceId)
    {
        await Task.CompletedTask;

        if (_serviceInstances.TryGetValue(serviceId, out var service))
        {
            return service;
        }

        return default(T);
    }

    protected string GenerateServiceId()
    {
        string id;

        do
        {
            id = _servicePrefix + Extensions.RandomString(10);
        } while (_serviceInstances.ContainsKey(id));

        return id;
    }
}
