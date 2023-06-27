using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Alerts;
using SmartFarmer.Helpers;
using SmartFarmer.Services.Alert;
using SmartFarmer.Services.Security;

namespace SmartFarmer.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FarmerAlertController : FarmerControllerBase
{
    private readonly ILogger<FarmerAlertController> _logger;
    private readonly ISmartFarmerAlertControllerService _alertProvider;

    public FarmerAlertController(
        ILogger<FarmerAlertController> logger,
        ISmartFarmerAlertControllerService alertProvider,
        ISmartFarmerUserAuthenticationService userManager)
        : base(userManager)
    {
        _logger = logger;
        _alertProvider = alertProvider;
    }
    
    [HttpGet("alertsInGarden")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IEnumerable<IFarmerAlert>>> GetAlertsByGarden(string id)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var alerts =
            await _alertProvider
                .GetFarmerAlertsByGardenIdAsync(userId, id);

        return Ok(alerts);
    }

    [HttpGet("alertsCountInGarden")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<int>> GetAlertsCountByGarden(string id)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var alerts =
            (await _alertProvider
                .GetFarmerAlertsByGardenIdAsync(userId, id))?.ToArray();

        return Ok(alerts?.Length);
    }

    [HttpGet("alertsCountToReadInGarden")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IEnumerable<IFarmerAlert>>> GetAlertsToReadCountByGarden(string id)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var alerts =
            (await _alertProvider
                .GetFarmerAlertsByGardenIdAsync(userId, id))?
                    .Where(a => !a.MarkedAsRead)
                    .ToArray();

        return Ok(alerts?.Length);
    }

    [HttpGet("alerts")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<IEnumerable<IFarmerAlert>>> GetAlertsByIds(string ids)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var alerts =
            await _alertProvider
                .GetFarmerAlertsByIdAsync(userId, ids.Split("#"));

        return Ok(alerts);
    }

    [HttpGet("markAlert")]
    [IsUserAuthorizedTo(Constants.AUTH_READ_GARDEN)]
    public async Task<ActionResult<bool>> MarkAlertAsRead(string alertId, bool read)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result =
            await _alertProvider
                .MarkFarmerAlertAsRead(userId, alertId, read);

        return Ok(result);
    }

    [HttpPost("createAlert")]
    [IsUserAuthorizedTo(Constants.AUTH_EDIT_GARDEN)]
    public async Task<ActionResult<string>> CreateAlert([FromBody]FarmerAlertRequestData data)
    {
        var userId = await GetUserIdByContext();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result =
            await _alertProvider
                .CreateFarmerAlert(userId, data);

        return Ok(result);
    }

}
