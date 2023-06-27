using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartFarmer.Data;
using SmartFarmer.DTOs.Security;
using SmartFarmer.Services.Security;

namespace SmartFarmer.Security;

public class SmartFarmerUserJWTAuthenticationService : SmartFarmerUserAuthenticationService
{
    private readonly SecretKey _secretKey;

    public SmartFarmerUserJWTAuthenticationService(IOptions<SecretKey> secretKey, ISmartFarmerRepository repository)
        : base(repository)
    {
        _secretKey = secretKey.Value;
    }

    protected override string GenerateToken(User user)
    {
        var issuer = _secretKey.Issuer;
        var audience = _secretKey.Audience;
        var key = Encoding.ASCII.GetBytes(_secretKey.Key);
        
        // https://jasonwatmore.com/post/2021/12/14/net-6-jwt-authentication-tutorial-with-example-api
        // generate token that is valid for 7 days
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sid, user.ID), // Guid.NewGuid().ToString()
                // new Claim(JwtRegisteredClaimNames.Sub, user.ID),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}