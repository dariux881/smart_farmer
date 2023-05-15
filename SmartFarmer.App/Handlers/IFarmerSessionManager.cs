using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Handlers;

public interface IFarmerSessionManager
{
    Task<bool> LoginAsync(CancellationToken token);
    string LoggedUserId { get; }
    string Token { get; }
}