using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartFarmer.Data;
using SmartFarmer.Services;

namespace SmartFarmer.Helpers;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class IsUserAuthorizedTo : Attribute, IAsyncAuthorizationFilter
{
    private string[] _authorizationIds;

    public IsUserAuthorizedTo(string authorizations)
    {
        _authorizationIds = authorizations.Split("|");
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var token = (string)context.HttpContext.Items[Constants.HEADER_AUTHENTICATION_TOKEN];
        if (token == null)
        {
            // not logged in
            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }

        var dbContext = context.HttpContext
            .RequestServices
            .GetService(typeof(SmartFarmerDbContext)) as SmartFarmerDbContext;

        var userManager = context.HttpContext
            .RequestServices
            .GetService(typeof(ISmartFarmerUserAuthenticationService)) as ISmartFarmerUserAuthenticationService;

        // getting user
        var userId = await userManager.GetLoggedUserIdByToken(token);
         if (userId == null)
        {
            // not logged in
            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }

        if (!await userManager.IsUserAuthorizedToAnyOf(userId, _authorizationIds))
        {
            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}