using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Helpers;
using SmartFarmer.Plants;
using SmartFarmer.Services;
using SmartFarmer.Tasks.Generic;

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
        [IsUserAuthorizedTo("readGround")]
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
        [IsUserAuthorizedTo("readGround")]
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
        [IsUserAuthorizedTo("readGround")]
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
        
        [HttpGet("plantsInGround")]
        [IsUserAuthorizedTo("readGround")]
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
        
        [HttpGet("plant")]
        public async Task<ActionResult<IFarmerPlant>> GetPlant(string id)
        {
            var plant = 
                await _groundProvider
                    .GetFarmerPlantByIdAsync(id);

            return Ok(plant);
        }

        [HttpGet("plants")]
        public async Task<ActionResult<IEnumerable<IFarmerPlant>>> GetPlants(string idsSplit)
        {
            var plant = 
                await _groundProvider
                    .GetFarmerPlantByIdsAsync(idsSplit.Split('#'));

            return Ok(plant);
        }

        [HttpGet("plan")]
        [IsUserAuthorizedTo("readGround")]
        public async Task<ActionResult<IFarmerPlan>> GetPlan(string id)
        {
            var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var userId = await _userManager.GetLoggedUserIdByToken(token);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var plan = 
                await _groundProvider
                    .GetFarmerPlanByIdForUserAsync(userId, id);

            return Ok(plan);
        }

        [HttpGet("plans")]
        [IsUserAuthorizedTo("readGround")]
        public async Task<ActionResult<IEnumerable<IFarmerPlant>>> GetPlans(string ids)
        {
            var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var userId = await _userManager.GetLoggedUserIdByToken(token);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var plans = 
                await _groundProvider
                    .GetFarmerPlanByIdsForUserAsync(userId, ids.Split('#'));

            return Ok(plans);
        }

        
        [HttpGet("steps")]
        [IsUserAuthorizedTo("readGround")]
        public async Task<ActionResult<IEnumerable<IFarmerPlant>>> GetSteps(string ids)
        {
            // var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

            // if (string.IsNullOrEmpty(token))
            //     return Unauthorized();

            // var userId = await _userManager.GetLoggedUserIdByToken(token);

            // if (string.IsNullOrEmpty(userId))
            //     return Unauthorized();

            var steps = 
                await _groundProvider
                    .GetFarmerPlanStepByIdsAsync(ids.Split('#'));

            return Ok(steps);
        }
    }
}
