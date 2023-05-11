//using SmartFarmer.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.SignalR;

namespace SmartFarmer.Hubs;

public class FarmerUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        return connection.User?.Claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sid)?.Value;
    }
}