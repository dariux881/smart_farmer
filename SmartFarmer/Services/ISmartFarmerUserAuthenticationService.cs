using System.Threading.Tasks;
using SmartFarmer.Data.Security;
using SmartFarmer.DTOs.Security;

namespace SmartFarmer.Services;

public interface ISmartFarmerUserAuthenticationService
{
    Task<string> GetLoggedUserIdByToken(string token);
    Task<UserLogin> GetLoggedUserById(string userId);

    Task<bool> IsUserAuthorizedTo(string userId, string authorizationId);
    Task<bool> IsUserAuthorizedToAnyOf(string userId, string[] authorizationIds);

    Task<LoginResponseData> LogInUser(string userName, string password, object[] parameters);
    Task LogOutUser(string token);

    Task<IFarmerSettings> GetUserSettings(string userId);
    Task<bool> SaveUserSettings(string userId, IFarmerSettings settings);
}
