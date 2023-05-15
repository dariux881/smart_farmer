using System.Linq;
using SmartFarmer.Handlers;

namespace SmartFarmer.Misc;

public class GroundUtils
{
    public static IFarmerGround GetGroundByPlan(string planId)
    {
        return FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true).Grounds.Values.FirstOrDefault(x => x.PlanIds.Contains(planId));
    }

    public static IFarmerGround GetGroundByAlert(string alertId)
    {
        return FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true).Grounds.Values.FirstOrDefault(x => x.AlertIds.Contains(alertId));
    }    
}