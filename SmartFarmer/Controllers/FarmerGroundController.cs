using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public FarmerGroundController(
            ILogger<FarmerGroundController> logger,
            ISmartFarmerGroundControllerService groundProvider)
        {
            _logger = logger;
            _groundProvider = groundProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IFarmerGround>>> Get(string token, string groundId)
        {
            //TODO add filter by logged user Id
            var grounds = await _groundProvider.GetFarmerGroundByIdAsync(groundId);

            return Ok(grounds);
        }
    }
}
