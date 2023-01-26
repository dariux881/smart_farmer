using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Authentication;
using SmartFarmer.Data;
using SmartFarmer.DTOs;
using SmartFarmer.Utils;

namespace SmartFarmer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FarmerGroundController : ControllerBase
    {
        private readonly ILogger<FarmerGroundController> _logger;
        private readonly ISmartFarmerGroundControllerService _groundProvider;
        private readonly ISmartFarmerUserManager _userManager;

        public FarmerGroundController(
            ILogger<FarmerGroundController> logger,
            ISmartFarmerGroundControllerService groundProvider,
            ISmartFarmerUserManager userManager)
        {
            _logger = logger;
            _groundProvider = groundProvider;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IFarmerGround>>> Get(string token)
        {
            var grounds = await _groundProvider.GetFarmerGroundByUserIdAsync(_userManager.GetUserIdByToken(token));

            return Ok(grounds);
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IFarmerGround>>> Get(string token, string groundId)
        {
            var grounds = 
                await _groundProvider
                    .GetFarmerGroundByIdForUserAsync(
                        _userManager.GetUserIdByToken(token),
                        groundId);

            return Ok(grounds);
        }
    }
}
