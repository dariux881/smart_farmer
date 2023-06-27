using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Data.Security;
using SmartFarmer.DTOs.Security;
using SmartFarmer.Helpers;
using SmartFarmer.Services.Security;

namespace SmartFarmer.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly ISmartFarmerUserAuthenticationService _userManager;

    public AuthenticationController(
        ILogger<AuthenticationController> logger,
        ISmartFarmerUserAuthenticationService userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    [HttpPost("LogIn")]
    public async Task<ActionResult<LoginResponseData>> LogIn([FromBody] LoginRequestData userLoginData)
    {
        //TODO encrypt
        var result = await _userManager.LogInUser(
            userLoginData.UserName, 
            userLoginData.Password, 
            userLoginData.Parameters);

        if (result == null || string.IsNullOrEmpty(result.Token)) {
            return Unauthorized();
        }

        return Ok(result);
    }
    
    [HttpGet("LogOut")]
    public async Task<ActionResult> LogOut(string token)
    {
        await _userManager.LogOutUser(token);
        return Ok();
    }

    [Authorize]
    [HttpGet("GetSettings")]
    public async Task<ActionResult<FarmerSettings>> GetUserSettings()
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var userSettings = await _userManager.GetUserSettings(userId);

        if (userSettings != null)
            return Ok(userSettings);

        return BadRequest();
    }
    
    [Authorize]
    [HttpPost("SaveSettings")]
    public async Task<ActionResult<bool>> SaveUserSettings([FromBody] FarmerSettings settings)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        return Ok(await _userManager.SaveUserSettings(userId, settings));
    }

    private async Task<string> GetUserIdByContext()
    {
        var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

        if (string.IsNullOrEmpty(token))
            return null;

        return await _userManager.GetLoggedUserIdByToken(token);
    }
}