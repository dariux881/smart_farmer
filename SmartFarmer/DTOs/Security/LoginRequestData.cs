namespace SmartFarmer.DTOs.Security;

public class LoginRequestData
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public object[] Parameters { get; set; }
}