using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Handlers;

public interface IFarmerLocalInformationManager
{

    ConcurrentDictionary<string, IFarmerGarden> Gardens { get; }

    Task InitializeGardensAsync(CancellationToken token);
    Task ReinitializeGardensAsync(CancellationToken token);

    void ClearLocalData(
        bool clearGardens = false,
        bool clearLoggedUser = false,
        bool clearToken = false);

    void PushVolatileData(string key, object data);
    object PickVolatileData(string key);
}