using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Handlers;

public interface IFarmerLocalInformationManager
{

    ConcurrentDictionary<string, IFarmerGround> Grounds { get; }

    Task InitializeGroundsAsync(CancellationToken token);
    Task ReinitializeGroundsAsync(CancellationToken token);

    void ClearLocalData(
        bool clearGrounds = false,
        bool clearLoggedUser = false,
        bool clearToken = false);
}