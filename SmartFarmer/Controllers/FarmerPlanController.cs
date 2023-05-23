using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Plants;
using SmartFarmer.Helpers;
using SmartFarmer.Services;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks;

namespace SmartFarmer.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FarmerPlanController : FarmerControllerBase
{
    private readonly ILogger<FarmerGroundController> _logger;
    private readonly ISmartFarmerGroundControllerService _groundProvider;

    public FarmerPlanController(
        ILogger<FarmerGroundController> logger,
        ISmartFarmerGroundControllerService groundProvider,
        ISmartFarmerUserAuthenticationService userManager)
        : base(userManager)
    {
        _logger = logger;
        _groundProvider = groundProvider;
    }


    [HttpGet("plan")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IFarmerPlan>> GetPlan(string id)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plan =
            await _groundProvider
                .GetFarmerPlanByIdForUserAsync(userId, id);

        return Ok(plan);
    }

    [HttpGet("plans")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IEnumerable<IFarmerPlant>>> GetPlans(string ids)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plans =
            await _groundProvider
                .GetFarmerPlanByIdsForUserAsync(userId, ids.Split('#'));

        return Ok(plans);
    }

    [HttpGet("steps")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IEnumerable<IFarmerPlanStep>>> GetSteps(string ids)
    {
        var steps =
            await _groundProvider
                .GetFarmerPlanStepByIdsAsync(ids.Split('#'));

        return Ok(steps);
    }

    [HttpPost("createPlan")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GROUND)]
    public async Task<ActionResult<string>> CreatePlan([FromBody] FarmerPlanRequestData plan)
    {
        if (plan == null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _groundProvider
            .AddPlan(userId, plan);

        if (!string.IsNullOrEmpty(result))
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("deletePlan")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GROUND)]
    public async Task<ActionResult<string>> DeletePlan([FromBody] string planId)
    {
        if (string.IsNullOrEmpty(planId))
        {
            throw new ArgumentNullException(nameof(planId));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _groundProvider
            .DeletePlan(userId, planId);

        if (result)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("createIrrigationPlan")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GROUND)]
    public async Task<ActionResult<string>> CreateIrrigationPlan(string groundId)
    {
        if (string.IsNullOrEmpty(groundId))
        {
            throw new ArgumentNullException(nameof(groundId));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _groundProvider
            .BuildIrrigationPlan(userId, groundId);

        if (!string.IsNullOrEmpty(result))
            return Ok(result);

        return BadRequest(result);
    }

}