using System.Threading.Tasks;
using SmartFarmer.DTOs.Security;

namespace SmartFarmer.Data;

public interface ISmartFarmerSecurityRepository
{
    Task<UserLogin> GetLoggedUserByToken(string token);
    Task<UserLogin> GetLoggedUserById(string userId);
    Task<User> GetUser(string userName, string password);

    Task LogInUser(UserLogin userLogin);
    Task LogOutUser(string token);
}
