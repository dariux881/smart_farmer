using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.Data.Plants;
using SmartFarmer.Data.Tasks;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Handlers;

////
// Partial class for Garden management
////
public partial class FarmerRequestHandler
{
    private static ConcurrentDictionary<string, object> _gardenElementsCache;

    static FarmerRequestHandler() {
        _gardenElementsCache = new ConcurrentDictionary<string, object>();
    }

    public static async Task<IEnumerable<IFarmerGarden>> GetGardensList(CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
            var response = await 
                httpReq
                    .GetAsync(
                        SmartFarmerApiConstants.GARDENS_BASE,
                        token);
            
            if (response == null || !response.IsSuccessStatusCode)
            {
                return null;
            }

            var gardenStr = await response.Content.ReadAsStringAsync(token);
            return gardenStr.Deserialize<List<FarmerGarden>>() as IEnumerable<IFarmerGarden>;
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return null;
        }
    }

    public static async Task<IFarmerGarden> GetGarden(string gardenId, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
            var response = await 
                httpReq
                    .GetAsync(
                        SmartFarmerApiConstants.GET_GARDEN,
                        token,
                        new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("id", gardenId) });
            
            if (response == null || !response.IsSuccessStatusCode)
            {
                return null;
            }

            var gardenStr = await response.Content.ReadAsStringAsync(token);
            var garden = gardenStr.Deserialize<FarmerGarden>();

            if (garden == null) throw new InvalidCastException("invalid garden");

            // getting plans
            await ResolvePlans(garden, garden.GetPlanIds(), token);
            // getting alerts
            await ResolveAlerts(garden, garden.GetAlertIds(), token);
            // getting plants
            await ResolvePlantsInstances(garden, garden.GetPlantIds(), token);

            return garden;
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return null;
        }
    }

    public static async Task<IFarmerPlant> GetPlant(string plantId, CancellationToken token)
    {
        if (_gardenElementsCache.TryGetValue(plantId, out var plantObj) && plantObj is FarmerPlant plantCache)
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

            _gardenElementsCache.TryAdd(plantId, plant);

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
                        SmartFarmerApiConstants.GET_PLANTS_IN_GARDEN,
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

    public static async Task<bool> NotifyPlanExecutionResult(FarmerPlanExecutionResult result, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
            var response = await 
                httpReq
                    .PostAsync(
                        SmartFarmerApiConstants.NOTIFY_PLAN_EXECUTION_RESULT,
                        result,
                        token);
            
            return response != null && response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return false;
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

    public static async Task<bool> NotifyDevicePositions(FarmerDevicePositionsRequestData positions, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
            var response = await 
                httpReq
                    .PostAsync(
                        SmartFarmerApiConstants.UPDATE_DEVICE_POSITIONS,
                        positions,
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

    private static async Task ResolvePlans(FarmerGarden garden, string[] ids, CancellationToken token)
    {
        var plans = await GetPlans(ids, token);

        if (plans != null && plans.Count() == ids.Length) 
        {
            garden.AddPlans(plans.ToList());
        }
    }

    private static async Task ResolveAlerts(FarmerGarden garden, string[] ids, CancellationToken token)
    {
        var alerts = await GetAlerts(ids, token);

        if (alerts != null)
        {
            garden.AddAlerts(alerts.ToList());
        }
    }

    private static async Task ResolvePlantsInstances(FarmerGarden garden, string[] ids, CancellationToken token)
    {
        var plants = await GetPlantsInstance(ids, token);

        if (plants != null && plants.Count() == ids.Length) 
        {
            garden.AddPlants(plants.ToList());
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
