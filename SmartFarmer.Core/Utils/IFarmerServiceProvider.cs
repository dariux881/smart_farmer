namespace SmartFarmer.Utils;

public interface IFarmerServiceProvider<T> where T : IFarmerService
{
    void AddFarmerService(T service);
    string GenerateServiceId();
    T GetFarmerService(string serviceId);
}
