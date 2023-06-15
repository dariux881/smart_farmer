using System.Linq;
using SmartFarmer.Handlers;

namespace SmartFarmer.Misc;

public class GardenUtils
{
    public static IFarmerGarden GetGardenByPlan(string planId)
    {
        return FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true).Gardens.Values.FirstOrDefault(x => x.PlanIds.Contains(planId));
    }

    public static IFarmerGarden GetGardenByAlert(string alertId)
    {
        return FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true).Gardens.Values.FirstOrDefault(x => x.AlertIds.Contains(alertId));
    }    
}