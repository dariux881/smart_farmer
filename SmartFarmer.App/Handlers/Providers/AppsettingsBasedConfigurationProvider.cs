using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SmartFarmer.Configurations;

namespace SmartFarmer.Handlers.Providers;

public class AppsettingsBasedConfigurationProvider : IFarmerConfigurationProvider
{
    private AppConfiguration _appConfiguration;
    private ApiConfiguration _apiConfiguration;
    private HubConnectionConfiguration _hubConfiguration;
    private GardensConfiguration _gardensConfiguration;
    private UserConfiguration _userConfiguration;

    public AppsettingsBasedConfigurationProvider(IConfiguration config)
    {
        _apiConfiguration = config.GetSection("ApiConfiguration").Get<ApiConfiguration>();
        _userConfiguration = config.GetSection("UserConfiguration").Get<UserConfiguration>();
        _appConfiguration = config.GetSection("AppConfiguration").Get<AppConfiguration>();
        _hubConfiguration = config.GetSection("HubConnectionConfiguration").Get<HubConnectionConfiguration>();
        _gardensConfiguration = config.GetSection("GardensConfiguration").Get<GardensConfiguration>();
    }

    public AppConfiguration GetAppConfiguration()
    {
        return _appConfiguration;
    }

    public ApiConfiguration GetApiConfiguration()
    {
        return _apiConfiguration;
    }

    public UserConfiguration GetUserConfiguration()
    {
        return _userConfiguration;
    }

    public GardenConfiguration GetGardenConfiguration(string gardenId)
    {
        return _gardensConfiguration.Configurations.FirstOrDefault(x => x.GardenId == gardenId);
    }

    public HubConnectionConfiguration GetHubConfiguration()
    {
        return _hubConfiguration;
    }
}