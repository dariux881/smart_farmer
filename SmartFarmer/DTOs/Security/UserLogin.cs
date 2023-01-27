
using System;
using SmartFarmer.Utils;

namespace SmartFarmer.DTOs.Security;

public class UserLogin : IFarmerService
{
    public string ID { get; set; }
    public string Token { get; set; }
    public string UserId { get; set; }
    public DateTime LoginDt { get; set; }
    public DateTime? LogoutDt { get; set; }
}