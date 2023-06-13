using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmer.Alerts;
using SmartFarmer.Helpers;
using SmartFarmer.Services;

namespace SmartFarmer.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FarmerAlertController : FarmerControllerBase
{
    private readonly ILogger<FarmerGroundController> _logger;
    private readonly ISmartFarmerGroundControllerService _groundProvider;

    public FarmerAlertController(
        ILogger<FarmerGroundController> logger,
        ISmartFarmerGroundControllerService groundProvider,
        ISmartFarmerUserAuthenticationService userManager)
        : base(userManager)
    {
        _logger = logger;
        _groundProvider = groundProvider;
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

}
