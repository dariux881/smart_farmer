namespace SmartFarmer.Data.Security;

public class LoginResponseData
{
    public LoginResponseData(string token, string userId, string errorMessage)
    {
        Token = token;
        UserId = userId;
        ErrorMessage = errorMessage;
    }

    public string Token { get; }
    public string UserId { get; }
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    public string ErrorMessage { get; }
}