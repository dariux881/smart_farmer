using System.Threading.Tasks;
using SmartFarmer.DTOs.Security;

namespace SmartFarmer.Data;

public interface ISmartFarmerSecurityRepository
{
    Task<UserLogin> GetLoggedUserByToken(string token);
    Task<UserLogin> GetLoggedUserById(string userId);
    Task<User> GetUser(string userName, string password);

    Task<bool> IsUserAuthorizedTo(string userId, string authorizationId);
    Task<bool> IsUserAuthorizedToAnyOf(string userId, string[] authorizationIds);

    Task LogInUser(UserLogin userLogin);
    Task LogOutUser(string token);

    Task<IFarmerSettings> GetUserSettings(string userId);
    Task<bool> SaveUserSettings(string userId, IFarmerSettings settings);
}
