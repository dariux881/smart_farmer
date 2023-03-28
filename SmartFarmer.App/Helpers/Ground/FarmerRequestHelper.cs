using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.Data.Alerts;
using SmartFarmer.Data.Plants;
using SmartFarmer.Data.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Helpers;

////
// Partial class for Ground management
////
public partial class FarmerRequestHelper
{
    public static async Task<IEnumerable<IFarmerGround>> GetGrounds(CancellationToken token)
    {
        var httpReq = new HttpRequest();

        var response = await 
            httpReq
                .GetAsync(
                    SmartFarmerApiConstants.GROUNDS_BASE,
                    token);
        
        if (response == null || !response.IsSuccessStatusCode)
        {
            return null;
        }

        var groundStr = await response.Content.ReadAsStringAsync(token);
        return groundStr.Deserialize<List<FarmerGround>>() as IEnumerable<IFarmerGround>;
    }

    public static async Task<IFarmerGround> GetGround(string groundId, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        var response = await 
            httpReq
                .GetAsync(
                    SmartFarmerApiConstants.GET_GROUND,
                    token,
                    new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("id", groundId) });
        
        if (response == null || !response.IsSuccessStatusCode)
        {
            return null;
        }

        var groundStr = await response.Content.ReadAsStringAsync(token);
        var ground = groundStr.Deserialize<FarmerGround>();

        if (ground == null) return null; //FIXME throw exception

        // getting plans
        await ResolvePlans(ground, ground.GetPlanIds(), token);

        // getting alerts
        await ResolveAlerts(ground, ground.GetAlertIds(), token);

        // getting plants
        await ResolvePlantsInstances(ground, ground.GetPlantIds(), token);

        return ground;
    }

    public static async Task<IEnumerable<IFarmerPlan>> GetPlans(string[] planIds, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        var response = await 
            httpReq
                .GetAsync(
                    SmartFarmerApiConstants.GET_PLANS,
                    token,
                    new KeyValuePair<string, string>[] { 
                        new KeyValuePair<string, string>(
                            "ids", 
                            planIds.Aggregate((p1, p2) => p1 + "#" + p2)) });
        
        if (response == null || !response.IsSuccessStatusCode)
        {
            return null;
        }

        var groundStr = await response.Content.ReadAsStringAsync(token);

        // resolve Plan
        return groundStr.Deserialize<List<FarmerPlan>>() as IEnumerable<IFarmerPlan>;
    }

    public static async Task<IEnumerable<IFarmerPlantInstance>> GetPlantsInstance(string[] plantIds, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        var response = await 
            httpReq
                .GetAsync(
                    SmartFarmerApiConstants.GET_PLANTS_IN_GROUND,
                    token,
                    new KeyValuePair<string, string>[] { 
                        new KeyValuePair<string, string>(
                            "idsSplit", 
                            plantIds.Aggregate((p1, p2) => p1 + "#" + p2)) });
        
        if (response == null || !response.IsSuccessStatusCode)
        {
            return null;
        }

        var groundStr = await response.Content.ReadAsStringAsync(token);

        //TODO resolve Plant
        return groundStr.Deserialize<List<FarmerPlantInstance>>() as IEnumerable<IFarmerPlantInstance>;
    }

    private static async Task ResolvePlans(FarmerGround ground, string[] ids, CancellationToken token)
    {
        var plans = await GetPlans(ids, token);

        if (plans != null && plans.Count() == ids.Length) 
        {
            ground.Plans.AddRange(plans as List<FarmerPlan>);
        }
    }

    private static async Task ResolveAlerts(FarmerGround ground, string[] ids, CancellationToken token)
    {
        foreach (var alertId in ids)
        {
            var alert = await GetAlert(alertId, token);
            if (alert != null)
            {
                ground.Alerts.Add(alert as FarmerAlert);
            }
        }
    }

    private static async Task ResolvePlantsInstances(FarmerGround ground, string[] ids, CancellationToken token)
    {
        var plants = await GetPlantsInstance(ids, token);

        if (plants != null && plants.Count() == ids.Length) 
        {
            ground.Plants.AddRange(plants as List<FarmerPlantInstance>);
        }
    }
}
