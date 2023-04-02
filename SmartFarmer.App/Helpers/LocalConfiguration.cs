using System.Collections.Generic;

namespace SmartFarmer.Helpers;

public class LocalConfiguration
{
    static LocalConfiguration() {
        LocalGroundIds = new List<string>();
        Grounds = new Dictionary<string, IFarmerGround>();
    }

    public static List<string> LocalGroundIds { get; set; }

    public static string LoggedUserId { get; set; }
    public static string Token { get; set; }
    public static Dictionary<string, IFarmerGround> Grounds { get; set; }
}