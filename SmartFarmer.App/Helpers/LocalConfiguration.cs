using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SmartFarmer.Helpers;

public class LocalConfiguration
{
    private static object tokenLock = new object();
    private static object userIdLock = new object();
    private static string loggedUserId;
    private static string token;

    static LocalConfiguration() {
        Grounds = new ConcurrentDictionary<string, IFarmerGround>();
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
    
    public static ConcurrentDictionary<string, IFarmerGround> Grounds { get; set; }

    public static void ClearLocalData(
        bool clearGrounds = false,
        bool clearLoggedUser = false,
        bool clearToken = false)
    {
        if (clearGrounds)
        {
            Grounds.Clear();
        }

        if (clearLoggedUser)
        {
            LoggedUserId = null;
        }

        if (clearToken)
        {
            Token = null;
        }
    }
}