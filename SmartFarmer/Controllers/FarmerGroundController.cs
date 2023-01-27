using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.DTOs.Security;
using SmartFarmer.Helpers;
using SmartFarmer.Services;

namespace SmartFarmer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class FarmerGroundController : ControllerBase
    {
        private readonly ILogger<FarmerGroundController> _logger;
        private readonly ISmartFarmerGroundControllerService _groundProvider;
        private readonly ISmartFarmerUserAuthenticationService _userManager;

        public FarmerGroundController(
            ILogger<FarmerGroundController> logger,
            ISmartFarmerGroundControllerService groundProvider,
            ISmartFarmerUserAuthenticationService userManager)
        {
            _logger = logger;
            _groundProvider = groundProvider;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IFarmerGround>>> GetAllGrounds()
        {
            var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var userId = await _userManager.GetLoggedUserIdByToken(token);
            var grounds = await _groundProvider.GetFarmerGroundByUserIdAsync(userId);

            return Ok(grounds);
        }
        
        [HttpGet("id")]
        public async Task<ActionResult<IEnumerable<IFarmerGround>>> Get(string groundId)
        {
            var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var grounds = 
                await _groundProvider
                    .GetFarmerGroundByIdForUserAsync(
                        await _userManager.GetLoggedUserIdByToken(token),
                        groundId);

            return Ok(grounds);
        }
    }
}
