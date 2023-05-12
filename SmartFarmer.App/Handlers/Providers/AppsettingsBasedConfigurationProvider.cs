using System.Linq;
using Microsoft.Extensions.Configuration;
using SmartFarmer.Configurations;

namespace SmartFarmer.Handlers.Providers;

public class AppsettingsBasedConfigurationProvider : IFarmerConfigurationProvider
{
    private AppConfiguration _appConfiguration;
    private ApiConfiguration _apiConfiguration;
    private HubConnectionConfiguration _hubConfiguration;
    private GroundsConfiguration _groundsConfiguration;
    private UserConfiguration _userConfiguration;

    public AppsettingsBasedConfigurationProvider(IConfiguration config)
    {
        _apiConfiguration = config.GetSection("ApiConfiguration").Get<ApiConfiguration>();
        _userConfiguration = config.GetSection("UserConfiguration").Get<UserConfiguration>();
        _appConfiguration = config.GetSection("AppConfiguration").Get<AppConfiguration>();
        _hubConfiguration = config.GetSection("HubConnectionConfiguration").Get<HubConnectionConfiguration>();
        _groundsConfiguration = config.GetSection("GroundsConfiguration").Get<GroundsConfiguration>();
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

    public GroundConfiguration GetGroundConfiguration(string groundId)
    {
        return _groundsConfiguration.Configurations.FirstOrDefault(x => x.GroundId == groundId);
    }

    public HubConnectionConfiguration GetHubConfiguration()
    {
        return _hubConfiguration;
    }
}