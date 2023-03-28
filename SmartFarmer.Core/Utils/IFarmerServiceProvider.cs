using System.Threading.Tasks;

namespace SmartFarmer.Utils;

public interface IFarmerServiceProvider<T> where T : IFarmerService
{
    Task<string> AddFarmerService(T service);
    Task<T> GetFarmerService(string serviceId);
}
