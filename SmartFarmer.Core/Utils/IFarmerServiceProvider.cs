namespace SmartFarmer.Utils;

public interface IFarmerServiceProvider<T> where T : IFarmerService
{
    bool AddFarmerService(T service);
    string GenerateServiceId();
    T GetFarmerService(string serviceId);
}
