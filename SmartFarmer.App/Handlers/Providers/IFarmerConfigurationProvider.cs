using SmartFarmer.Configurations;
using SmartFarmer.Handlers;

namespace SmartFarmer.Handlers.Providers;

public interface IFarmerConfigurationProvider
{
    GroundConfiguration GetGroundConfiguration(string groundId);
    AppConfiguration GetAppConfiguration();
    ApiConfiguration GetApiConfiguration();
    LocalConfiguration GetLocalConfiguration();
    UserConfiguration GetUserConfiguration();
    HubConnectionConfiguration GetHubConfiguration();
}