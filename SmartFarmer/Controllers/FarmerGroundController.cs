using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Helpers;
using SmartFarmer.Plants;
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

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var grounds = await _groundProvider.GetFarmerGroundByUserIdAsync(userId);

            return Ok(grounds);
        }
        
        [HttpGet("ground")]
        public async Task<ActionResult<IFarmerGround>> GetGround(string id)
        {
            var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var userId = await _userManager.GetLoggedUserIdByToken(token);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var grounds = 
                await _groundProvider
                    .GetFarmerGroundByIdForUserAsync(
                        userId,
                        id);

            return Ok(grounds);
        }
        
        [HttpGet("plantInGround")]
        public async Task<ActionResult<IFarmerPlantInstance>> GetPlantInstance(string id)
        {
            var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var userId = await _userManager.GetLoggedUserIdByToken(token);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var plant = 
                await _groundProvider
                    .GetFarmerPlantInstanceByIdForUserAsync(
                        userId,
                        id);

            return Ok(plant);
        }
        
        [HttpGet("plantInGround")]
        public async Task<ActionResult<IEnumerable<IFarmerPlantInstance>>> GetPlantsInstance(string idsSplit)
        {
            var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var userId = await _userManager.GetLoggedUserIdByToken(token);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var plant = 
                await _groundProvider
                    .GetFarmerPlantInstanceByIdsForUserAsync(
                        userId,
                        idsSplit.Split('#'));

            return Ok(plant);
        }
    }
}
