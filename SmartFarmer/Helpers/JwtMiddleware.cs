using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartFarmer.Security;
using SmartFarmer.Services.Security;

namespace SmartFarmer.Helpers;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecretKey _appSettings;

    public JwtMiddleware(RequestDelegate next, IOptions<SecretKey> appSettings)
    {
        _next = next;
        _appSettings = appSettings.Value;
    }

    public async Task Invoke(HttpContext context, ISmartFarmerUserAuthenticationService userService)
    {
        var authHeader = context.Request.Headers["Authorization"];
        var tokenHeader = 
            authHeader.Any(x => x.Contains("Bearer")) ? 
                authHeader.FirstOrDefault(x => x.Contains("Bearer")) :
                authHeader.FirstOrDefault();

        var token = tokenHeader?
            .Replace("Bearer", "")
            .Split(" ")
            .Last();

        if (token != null)
            attachUserToContext(context, userService, token);

        await _next(context);
    }

    private void attachUserToContext(HttpContext context, ISmartFarmerUserAuthenticationService userService, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Key);
            var claimsPrincipal = tokenHandler.ValidateToken(
                token, 
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                },
                out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            context.User = claimsPrincipal;

            // var userId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sid).Value;

            // attach user to context on successful jwt validation
            context.Items[Constants.HEADER_AUTHENTICATION_TOKEN] = token;
        }
        catch
        {
            // do nothing if jwt validation fails
            // user is not attached to context so request won't have access to secure routes
        }
    }
}