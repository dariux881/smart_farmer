using System;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.DTOs.Security;

namespace SmartFarmer.Services;

public abstract class SmartFarmerUserAuthenticationService : ISmartFarmerUserAuthenticationService
{
    private readonly ISmartFarmerRepository _repository;

    public SmartFarmerUserAuthenticationService(ISmartFarmerRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> GetLoggedUserIdByToken(string token)
    {
        return (await _repository.GetLoggedUserByToken(token))?.UserId;
    }

    public async Task<UserLogin> GetLoggedUserById(string userId)
    {
        return await _repository.GetLoggedUserById(userId);
    }

    public async Task<bool> IsUserAuthorizedTo(string userId, string authorizationId)
    {
        return await _repository.IsUserAuthorizedTo(userId, authorizationId);
    }

    public async Task<bool> IsUserAuthorizedToAnyOf(string userId, string[] authorizationIds)
    {
        return await _repository.IsUserAuthorizedToAnyOf(userId, authorizationIds);
    }

    public async Task<string> LogInUser(string userName, string password, object[] parameters)
    {
        // check if user exists
        var checkedUser = await _repository.GetUser(userName, password);

        if (checkedUser == null)
            return null;

        // verify if user is already logged and reuse token
        var checkLoggedUser = await GetLoggedUserById(checkedUser.ID);

        if (checkLoggedUser != null)
            return checkLoggedUser.Token;

        // generate new token. Insert logged user
        var newToken = GenerateToken(checkedUser);

        var now = DateTime.UtcNow;

        await 
            _repository
                .LogInUser(
                    new UserLogin 
                    { 
                        ID = checkedUser.ID + "_" + now.ToString("G"),
                        LoginDt = now, 
                        Token = newToken, 
                        UserId = checkedUser.ID, 
                        LogoutDt = null 
                    });

        return newToken;
    }

    public async Task LogOutUser(string token)
    {
        // verify if user is already logged
        await _repository.LogOutUser(token);
    }
    
    public async Task<IFarmerSettings> GetUserSettings(string userId)
    {
        var settings = await _repository.GetUserSettings(userId);

        if (settings == null)
        {
            settings = new DefaultFarmerSettings();
        }

        return settings;
    }

    public async Task<bool> SaveUserSettings(string userId, IFarmerSettings settings)
    {
        return await _repository.SaveUserSettings(userId, settings);
    }

    protected abstract string GenerateToken(User user);
}
