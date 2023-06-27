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
public class FarmerGardenController : FarmerControllerBase
{
    private readonly ILogger<FarmerGardenController> _logger;
    private readonly ISmartFarmerGardenControllerService _gardenProvider;
    private readonly ISmartFarmerPlantControllerService _plantProvider;

    public FarmerGardenController(
        ILogger<FarmerGardenController> logger,
        ISmartFarmerGardenControllerService gardenProvider,
        ISmartFarmerPlantControllerService plantProvider,
        ISmartFarmerUserAuthenticationService userManager)
        : base(userManager)
    {
        _logger = logger;
        _gardenProvider = gardenProvider;
        _plantProvider = plantProvider;
    }

    [HttpGet]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IEnumerable<IFarmerGarden>>> GetAllGardens()
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var gardens = await _gardenProvider.GetFarmerGardenByUserIdAsync(userId);

        return Ok(gardens);
    }

    [HttpGet("garden")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IFarmerGarden>> GetGarden(string id)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var gardens =
            await _gardenProvider
                .GetFarmerGardenByIdForUserAsync(
                    userId,
                    id);

        return Ok(gardens);
    }

    [HttpGet("plantInGarden")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IFarmerPlantInstance>> GetPlantInstance(string id)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plant =
            await _plantProvider
                .GetFarmerPlantInstanceByIdForUserAsync(
                    userId,
                    id);

        return Ok(plant);
    }

    [HttpGet("plantsInGarden")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IEnumerable<IFarmerPlantInstance>>> GetPlantsInstance(string idsSplit)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plant =
            await _plantProvider
                .GetFarmerPlantInstanceByIdsForUserAsync(
                    userId,
                    idsSplit.Split('#'));

        return Ok(plant);
    }

    [HttpGet("irrigationHistory")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IrrigationHistory>> IrrigationHistoryByPlant(string plantId)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var plant =
            await _plantProvider
                .GetFarmerIrrigationHistoryByPlantAsync(
                    userId,
                    plantId);

        return Ok(plant);
    }

    [HttpPost("markIrrigation")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GARDEN)]
    public async Task<ActionResult<bool>> MarkIrrigation([FromBody] FarmerPlantIrrigationInstance irrigation)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        
        var result = 
            await _plantProvider
                .MarkIrrigationInstance(userId, irrigation);

        return Ok(result);
    }

    [HttpGet("plant")]
    public async Task<ActionResult<IFarmerPlant>> GetPlant(string id)
    {
        var plant =
            await _plantProvider
                .GetFarmerPlantByIdAsync(id);

        return Ok(plant);
    }

    [HttpGet("plants")]
    public async Task<ActionResult<IEnumerable<IFarmerPlant>>> GetPlants(string idsSplit)
    {
        var plant =
            await _plantProvider
                .GetFarmerPlantByIdsAsync(idsSplit.Split('#'));

        return Ok(plant);
    }

    [HttpPost("createGarden")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GARDEN)]
    public async Task<ActionResult<string>> CreateGarden([FromBody] FarmerGardenRequestData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        
        var result = await _gardenProvider
            .CreateFarmerGarden(userId, data);

        if (result != null)
            return Ok(result.ID);

        return BadRequest(null);
    }

    [HttpPost("addPlant")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GARDEN)]
    public async Task<ActionResult<bool>> AddPlant([FromBody] FarmerPlantRequestData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _plantProvider
            .AddFarmerPlantInstance(userId, data);

        if (result)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("notifyPositions")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GARDEN)]
    public async Task<ActionResult> NotifyPositions([FromBody] FarmerDevicePositionsRequestData positions)
    {
        if (positions == null)
        {
            throw new ArgumentNullException(nameof(positions));
        }

        if (string.IsNullOrEmpty(positions.GardenId))
        {
            return BadRequest();
        }

        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _gardenProvider
            .NotifyDevicePositions(userId, positions);

        if (result)
        {
            return BadRequest();
        }

        return Ok();
    }

    [HttpGet("devicePositionHistory")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IEnumerable<FarmerDevicePosition>>> GetDevicePositionHistory(string gardenId, string runId = null)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _gardenProvider
            .GetDeviceDevicePositionHistory(userId, gardenId, runId);

        if (result == null)
        {
            return BadRequest(null);
        }

        return Ok(result);
    }
}
