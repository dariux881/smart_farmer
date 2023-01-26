namespace SmartFarmer.Authentication;

public interface ISmartFarmerUserManager
{
    string GetUserIdByToken(string token);
}