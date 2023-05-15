using System.Collections.Concurrent;

namespace SmartFarmer.Configurations;

public class LocalConfiguration
{
    private static object tokenLock = new object();
    private static object userIdLock = new object();
    private static object appConfigLock = new object();
    private static string loggedUserId;
    private static string token;

    static LocalConfiguration() {
    }

    public static string LoggedUserId 
    { 
        get {
            lock(userIdLock)
            {
                return loggedUserId;
            }
        }

        set
        {
            lock(userIdLock)
            {
                loggedUserId = value;
            }
        }
    }

    public static string Token
    { 
        get {
            lock(tokenLock)
            {
                return token;
            }
        }

        set
        {
            lock(tokenLock)
            {
                token = value;
            }
        }
    }
}