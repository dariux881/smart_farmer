using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Handlers.Providers;
using SmartFarmer.Misc;

namespace SmartFarmer.Handlers;

public class FarmerSessionManager : IFarmerSessionManager
{
    private readonly IFarmerConfigurationProvider _configProvider;
    private readonly IFarmerAppCommunicationHandler _communicationHandler;

    public FarmerSessionManager()
    {
        _configProvider = FarmerServiceLocator.GetService<IFarmerConfigurationProvider>(true);
        _communicationHandler = FarmerServiceLocator.GetService<IFarmerAppCommunicationHandler>(true);
    }

    public string LoggedUserId { get; private set; }

    public string Token { get; private set; }

    public async Task<bool> LoginAsync(CancellationToken token)
    {

        var loginResult = await LoginCoreAsync(token);
        if (!loginResult)
        {
            //TODO retry in case of failure
            SmartFarmerLog.Error($"Login failed for user {_configProvider.GetUserConfiguration().UserName}. Stopping manager");
            return false;
        }

        //TODO start token refresh job

        _communicationHandler.NotifyNewLoggedUser();

        return true;
    }

    private async Task<bool> LoginCoreAsync(CancellationToken token)
    {
        var user = new Data.Security.LoginRequestData() {
                UserName = _configProvider.GetUserConfiguration()?.UserName, 
                Password = _configProvider.GetUserConfiguration()?.Password
            };

        // login
        var loginResponse = await FarmerRequestHandler.Login(
            user, 
            token);

        // save login result
        if (loginResponse == null || !loginResponse.IsSuccess)
        {
            SmartFarmerLog.Error("Invalid login for user " + user.UserName + " error: " + loginResponse?.ErrorMessage);
            return false;
        }

        LoggedUserId = loginResponse.UserId;
        Token = loginResponse.Token;

        return !string.IsNullOrEmpty(Token);
    }
}