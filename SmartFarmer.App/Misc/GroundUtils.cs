using System.Linq;
using SmartFarmer.Configurations;

namespace SmartFarmer.Misc;

public class GroundUtils
{

    public static IFarmerGround GetGroundByPlan(string planId)
    {
        return LocalConfiguration.Grounds.Values.FirstOrDefault(x => x.PlanIds.Contains(planId));
    }

    public static IFarmerGround GetGroundByAlert(string alertId)
    {
        return LocalConfiguration.Grounds.Values.FirstOrDefault(x => x.AlertIds.Contains(alertId));
    }    
}