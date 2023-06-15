using SmartFarmer.Configurations;
using SmartFarmer.Handlers;

namespace SmartFarmer.Handlers.Providers;

public interface IFarmerConfigurationProvider
{
    GardenConfiguration GetGardenConfiguration(string gardenId);
    AppConfiguration GetAppConfiguration();
    ApiConfiguration GetApiConfiguration();
    UserConfiguration GetUserConfiguration();
    HubConnectionConfiguration GetHubConfiguration();
}