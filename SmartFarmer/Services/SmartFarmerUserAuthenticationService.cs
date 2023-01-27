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
    
    protected abstract string GenerateToken(User user);
}
