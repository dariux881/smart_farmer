using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartFarmer.Helpers;
using SmartFarmer.Services.Security;

namespace SmartFarmer.Controllers;

public class FarmerControllerBase : ControllerBase
{
    private readonly ISmartFarmerUserAuthenticationService _userManager;

    public FarmerControllerBase(
        ISmartFarmerUserAuthenticationService userManager)
    {
        _userManager = userManager;
    }

    protected async Task<string> GetUserIdByContext()
    {
        var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

        if (string.IsNullOrEmpty(token))
            return null;

        return await _userManager.GetLoggedUserIdByToken(token);
    }
}
