using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Helpers;
using SmartFarmer.Services.Security;
using SmartFarmer.Services.AI;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FarmerAIController : FarmerControllerBase
{
    private readonly ILogger<FarmerAIController> _logger;
    private readonly ISmartFarmerAIControllerService _aiService;

    public FarmerAIController(
        ILogger<FarmerAIController> logger,
        ISmartFarmerAIControllerService aiService,
        ISmartFarmerUserAuthenticationService userManager)
        : base(userManager)
    {
        _logger = logger;
        _aiService = aiService;
    }

    [HttpGet("GetPlanForPlant")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IFarmerPlan>> GetPlanToAnalysePlant(string plantId)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plan =
            await _aiService
                .GenerateHoverPlan(userId, plantId);

        return Ok(plan);
    }
}