using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Plants;
using SmartFarmer.Helpers;
using SmartFarmer.Services.Plan;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks;
using SmartFarmer.Services.Security;

namespace SmartFarmer.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FarmerPlanController : FarmerControllerBase
{
    private readonly ILogger<FarmerPlanController> _logger;
    private readonly ISmartFarmerPlanControllerService _planProvider;

    public FarmerPlanController(
        ILogger<FarmerPlanController> logger,
        ISmartFarmerPlanControllerService planProvider,
        ISmartFarmerUserAuthenticationService userManager)
        : base(userManager)
    {
        _logger = logger;
        _planProvider = planProvider;
    }

    [HttpGet("")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IEnumerable<string>>> GetPlansInGarden(string gardenId)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plan =
            await _planProvider
                .GetFarmerPlanIdsInGardenAsync(userId, gardenId);

        return Ok(plan);
    }

    [HttpGet("plan")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IFarmerPlan>> GetPlan(string id)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plan =
            await _planProvider
                .GetFarmerPlanByIdForUserAsync(userId, id);

        return Ok(plan);
    }

    [HttpGet("plans")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IEnumerable<IFarmerPlant>>> GetPlans(string ids)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plans =
            await _planProvider
                .GetFarmerPlanByIdsForUserAsync(userId, ids.Split('#'));

        return Ok(plans);
    }

    [HttpGet("steps")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IEnumerable<IFarmerPlanStep>>> GetSteps(string ids)
    {
        var steps =
            await _planProvider
                .GetFarmerPlanStepByIdsAsync(ids.Split('#'));

        return Ok(steps);
    }

    [HttpPost("createPlan")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GARDEN)]
    public async Task<ActionResult<string>> CreatePlan([FromBody] FarmerPlanRequestData plan)
    {
        if (plan == null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _planProvider
            .AddPlan(userId, plan);

        if (!string.IsNullOrEmpty(result))
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("deletePlan")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GARDEN)]
    public async Task<ActionResult<string>> DeletePlan(string planId)
    {
        if (string.IsNullOrEmpty(planId))
        {
            throw new ArgumentNullException(nameof(planId));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _planProvider
            .DeletePlan(userId, planId);

        if (result)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("createIrrigationPlan")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GARDEN)]
    public async Task<ActionResult<string>> CreateIrrigationPlan(string gardenId)
    {
        if (string.IsNullOrEmpty(gardenId))
        {
            throw new ArgumentNullException(nameof(gardenId));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _planProvider
            .BuildIrrigationPlan(userId, gardenId);

        if (!string.IsNullOrEmpty(result))
            return Ok(result);

        return BadRequest(result);
    }

    
    [HttpPost("planExecutionResult")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GARDEN)]
    public async Task<ActionResult> SendPlanExecutionResult([FromBody] FarmerPlanExecutionResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _planProvider
            .AnalysePlanResult(userId, result);

        return Ok();
    }

}