using System.Threading.Tasks;
using SmartFarmer.DTOs.Security;

namespace SmartFarmer.Services;

public interface ISmartFarmerUserAuthenticationService
{
    Task<string> GetLoggedUserIdByToken(string token);
    Task<UserLogin> GetLoggedUserById(string userId);

    Task<string> LogInUser(string userName, string password, object[] parameters);
    Task LogOutUser(string token);
}
