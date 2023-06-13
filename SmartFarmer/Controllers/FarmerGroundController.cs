using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Plants;
using SmartFarmer.Helpers;
using SmartFarmer.Services;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.Movement;
using SmartFarmer.DTOs.Movements;

namespace SmartFarmer.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FarmerGroundController : FarmerControllerBase
{
    private readonly ILogger<FarmerGroundController> _logger;
    private readonly ISmartFarmerGroundControllerService _groundProvider;

    public FarmerGroundController(
        ILogger<FarmerGroundController> logger,
        ISmartFarmerGroundControllerService groundProvider,
        ISmartFarmerUserAuthenticationService userManager)
        : base(userManager)
    {
        _logger = logger;
        _groundProvider = groundProvider;
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
}
