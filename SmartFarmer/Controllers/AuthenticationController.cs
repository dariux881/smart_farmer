using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.DTOs.Security;
using SmartFarmer.Services;

namespace SmartFarmer.Controllers
{
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

        [HttpPost]
        public async Task<ActionResult<string>> LogIn([FromBody] LoginRequestData userLoginData)
        {
            var token = await _userManager.LogInUser(userLoginData.UserName, userLoginData.Password, userLoginData.Parameters);

            if (string.IsNullOrEmpty(token)) {
                return Unauthorized();
            }

            return Ok(token);
        }
        
        [HttpGet]
        public async Task<ActionResult> LogOut(string token)
        {
            await _userManager.LogOutUser(token);
            return Ok();
        } 
    }
}