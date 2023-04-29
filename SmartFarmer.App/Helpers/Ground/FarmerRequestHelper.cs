using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.Data.Alerts;
using SmartFarmer.Data.Plants;
using SmartFarmer.Data.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Helpers;

////
// Partial class for Ground management
////
public partial class FarmerRequestHelper
{
    private static ConcurrentDictionary<string, object> _groundElementsCache;

    static FarmerRequestHelper() {
        _groundElementsCache = new ConcurrentDictionary<string, object>();
    }

    public static async Task<IEnumerable<IFarmerGround>> GetGroundsList(CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
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
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return null;
        }
    }

    public static async Task<IFarmerGround> GetGround(string groundId, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
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

            if (ground == null) throw new InvalidCastException("invalid ground");

            // getting plans
            await ResolvePlans(ground, ground.GetPlanIds(), token);
            // getting alerts
            await ResolveAlerts(ground, ground.GetAlertIds(), token);
            // getting plants
            await ResolvePlantsInstances(ground, ground.GetPlantIds(), token);

            return ground;
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return null;
        }
    }

    public static async Task<IFarmerPlant> GetPlant(string plantId, CancellationToken token)
    {
        if (_groundElementsCache.TryGetValue(plantId, out var plantObj) && plantObj is FarmerPlant plantCache)
        {
            await Task.CompletedTask;
            return plantCache as IFarmerPlant;
        }

        var httpReq = new HttpRequest();

        try
        {
            var response = await 
                httpReq
                    .GetAsync(
                        SmartFarmerApiConstants.GET_PLANT,
                        token,
                        new KeyValuePair<string, string>[] { 
                            new KeyValuePair<string, string>("id", plantId) });
            
            var plantStr = await response.Content.ReadAsStringAsync(token);
            var plant = plantStr.Deserialize<FarmerPlant>();

            if (plant == null) return null;

            _groundElementsCache.TryAdd(plantId, plant);

            return plant;
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);

            return null;
        }
    }

    public static async Task<IEnumerable<IFarmerPlantInstance>> GetPlantsInstance(string[] plantIds, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
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

            var plantsStr = await response.Content.ReadAsStringAsync(token);
            var plants = plantsStr.Deserialize<List<FarmerPlantInstance>>() as IEnumerable<IFarmerPlantInstance>;

            // resolving Plants
            foreach (var plant in plants)
            {
                await ResolvePlant(plant as FarmerPlantInstance, plant.PlantKindID, token);
            }

            return plants;
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);

            return null;
        }
    }

    public static async Task<IrrigationHistory> GetPlantIrrigationHistory(string plantId, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
            var response = await 
                httpReq
                    .GetAsync(
                        SmartFarmerApiConstants.GET_PLANT_IRRIGATION_HISTORY,
                        token,
                        new KeyValuePair<string, string>[] { 
                            new KeyValuePair<string, string>(
                                "plantId", plantId) });
            
            if (response == null || !response.IsSuccessStatusCode)
            {
                return null;
            }

            var plantStr = await response.Content.ReadAsStringAsync(token);

            // resolve Plan
            return plantStr.Deserialize<IrrigationHistory>() as IrrigationHistory;
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return null;
        }
    }

    public static async Task<bool> MarkIrrigationInstance(FarmerPlantIrrigationInstance step, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
            var response = await 
                httpReq
                    .PostAsync(
                        SmartFarmerApiConstants.SET_PLANT_IRRIGATION_STEP, step, token);
            
            return response != null && response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return false;
        }
    }

    public static async Task<IEnumerable<IFarmerPlan>> GetPlans(string[] planIds, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
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

            var planStr = await response.Content.ReadAsStringAsync(token);

            // resolve Plan
            return planStr.Deserialize<List<FarmerPlan>>() as IEnumerable<IFarmerPlan>;
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);

            return null;
        }
    }

    public static async Task<bool> NotifyDevicePosition(FarmerDevicePositionRequestData position, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
            var response = await 
                httpReq
                    .PostAsync(
                        SmartFarmerApiConstants.UPDATE_DEVICE_POSITION,
                        position,
                        token,
                        null);
            
            return response != null && response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return false;
        }
    }

    private static async Task ResolvePlans(FarmerGround ground, string[] ids, CancellationToken token)
    {
        var plans = await GetPlans(ids, token);

        if (plans != null && plans.Count() == ids.Length) 
        {
            ground.AddPlans(plans.ToList());
        }
    }

    private static async Task ResolveAlerts(FarmerGround ground, string[] ids, CancellationToken token)
    {
        var alerts = await GetAlerts(ids, token);

        if (alerts != null)
        {
            ground.AddAlerts(alerts.ToList());
        }
    }

    private static async Task ResolvePlantsInstances(FarmerGround ground, string[] ids, CancellationToken token)
    {
        var plants = await GetPlantsInstance(ids, token);

        if (plants != null && plants.Count() == ids.Length) 
        {
            ground.AddPlants(plants.ToList());
        }
    }

    private static async Task ResolvePlant(FarmerPlantInstance plantInstance, string plantId, CancellationToken token)
    {
        var plant = await GetPlant(plantId, token) as FarmerPlant;
        
        if (plant != null)
        {
            plantInstance.Plant = plant;
        }
    }
}
