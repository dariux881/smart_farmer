using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Alerts;
using SmartFarmer.Plants;
using SmartFarmer.Helpers;
using SmartFarmer.Services;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.Movement;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.Tasks;

namespace SmartFarmer.Controllers;

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
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IEnumerable<IFarmerGround>>> GetAllGrounds()
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var grounds = await _groundProvider.GetFarmerGroundByUserIdAsync(userId);

        return Ok(grounds);
    }

    [HttpGet("ground")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IFarmerGround>> GetGround(string id)
    {
        var userId = await GetUserIdByContext();

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
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IFarmerPlantInstance>> GetPlantInstance(string id)
    {
        var userId = await GetUserIdByContext();

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
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IEnumerable<IFarmerPlantInstance>>> GetPlantsInstance(string idsSplit)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plant =
            await _groundProvider
                .GetFarmerPlantInstanceByIdsForUserAsync(
                    userId,
                    idsSplit.Split('#'));

        return Ok(plant);
    }

    [HttpGet("irrigationHistory")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IrrigationHistory>> IrrigationHistoryByPlant(string plantId)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plant =
            await _groundProvider
                .GetFarmerIrrigationHistoryByPlantAsync(
                    userId,
                    plantId);

        return Ok(plant);
    }

    [HttpPost("markIrrigation")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GROUND)]
    public async Task<ActionResult<bool>> MarkIrrigation([FromBody] FarmerPlantIrrigationInstance irrigation)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        
        var result = 
            await _groundProvider
                .MarkIrrigationInstance(userId, irrigation);

        return Ok(result);
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

    [HttpGet("alertsInGround")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IEnumerable<IFarmerAlert>>> GetAlertsByGround(string id)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var alerts =
            await _groundProvider
                .GetFarmerAlertsByGroundIdAsync(userId, id);

        return Ok(alerts);
    }

    [HttpGet("alertsCountInGround")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<int>> GetAlertsCountByGround(string id)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var alerts =
            (await _groundProvider
                .GetFarmerAlertsByGroundIdAsync(userId, id))?.ToArray();

        return Ok(alerts?.Length);
    }

    [HttpGet("alertsCountToReadInGround")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IEnumerable<IFarmerAlert>>> GetAlertsToReadCountByGround(string id)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var alerts =
            (await _groundProvider
                .GetFarmerAlertsByGroundIdAsync(userId, id))?
                    .Where(a => !a.MarkedAsRead)
                    .ToArray();

        return Ok(alerts?.Length);
    }

    [HttpGet("alerts")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IEnumerable<IFarmerAlert>>> GetAlertsByIds(string ids)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var alerts =
            await _groundProvider
                .GetFarmerAlertsByIdAsync(userId, ids.Split("#"));

        return Ok(alerts);
    }

    [HttpGet("markAlert")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<bool>> MarkAlertAsRead(string alertId, bool read)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result =
            await _groundProvider
                .MarkFarmerAlertAsRead(userId, alertId, read);

        return Ok(result);
    }

    [HttpPost("createAlert")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GROUND)]
    public async Task<ActionResult<string>> CreateAlert([FromBody]FarmerAlertRequestData data)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result =
            await _groundProvider
                .CreateFarmerAlert(userId, data);

        return Ok(result);
    }

    [HttpPost("createGround")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GROUND)]
    public async Task<ActionResult<string>> CreateGround([FromBody] FarmerGroundRequestData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        
        var result = await _groundProvider
            .CreateFarmerGround(userId, data);

        if (result != null)
            return Ok(result.ID);

        return BadRequest(null);
    }

    [HttpPost("addPlant")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GROUND)]
    public async Task<ActionResult<bool>> AddPlant([FromBody] FarmerPlantRequestData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _groundProvider
            .AddFarmerPlantInstance(userId, data);

        if (result)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("createPlan")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GROUND)]
    public async Task<ActionResult<string>> CreateIrrigationPlan([FromBody] FarmerPlanRequestData plan)
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

    [HttpPost("notifyPositions")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GROUND)]
    public async Task<ActionResult> NotifyPositions([FromBody] FarmerDevicePositionsRequestData positions)
    {
        if (positions == null)
        {
            throw new ArgumentNullException(nameof(positions));
        }

        if (string.IsNullOrEmpty(positions.GroundId))
        {
            return BadRequest();
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _groundProvider
            .NotifyDevicePositions(userId, positions);

        if (result)
        {
            return BadRequest();
        }

        return Ok();
    }

    [HttpGet("devicePositionHistory")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GROUND)]
    public async Task<ActionResult<IEnumerable<FarmerDevicePosition>>> GetDevicePositionHistory(string groundId, string runId = null)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _groundProvider
            .GetDeviceDevicePositionHistory(userId, groundId, runId);

        if (result == null)
        {
            return BadRequest(null);
        }

        return Ok(result);
    }

    private async Task<string> GetUserIdByContext()
    {
        var token = (string)HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];

        if (string.IsNullOrEmpty(token))
            return null;

        return await _userManager.GetLoggedUserIdByToken(token);
    }
}
