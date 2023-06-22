using System;
using System.Collections.Generic;
using System.Linq;
using SmartFarmer.Data.Plants;
using SmartFarmer.Data.Tasks;
using SmartFarmer.Data.Alerts;
using Newtonsoft.Json;
using SmartFarmer.Alerts;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;
using System.Threading.Tasks;
using System.Threading;
using SmartFarmer.Misc;

namespace SmartFarmer.Data;

public class FarmerGarden : IFarmerGarden
{
    private string[] plantIdsToResolve;
    private string[] planIdsToResolve;
    private string[] alertIdsToResolve;

    private List<IFarmerAlert> _alerts;
    private List<IFarmerPlantInstance> _plants;
    private List<IFarmerPlan> _plans;
    private SemaphoreSlim _planExecSem;


    public FarmerGarden()
    {
        _alerts = new List<IFarmerAlert>();
        _plants = new List<IFarmerPlantInstance>();
        _plans = new List<IFarmerPlan>();

        _planExecSem = new SemaphoreSlim(1);
    }

    [JsonConstructor]
    public FarmerGarden(        
        string[] plantIds,
        string[] planIds,
        string[] alertIds)
        : this()
    {
        plantIdsToResolve = plantIds;
        planIdsToResolve = planIds;
        alertIdsToResolve = alertIds;
    }

    public string ID { get; set; }
    public string GardenName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string UserID { get; set; }

    public IReadOnlyList<IFarmerPlantInstance> Plants => _plants.AsReadOnly();
    public IReadOnlyList<string> PlantIds => _plants.Select(x => x.ID).ToList().AsReadOnly();
    public IReadOnlyList<IFarmerPlan> Plans => _plans.AsReadOnly();
    public IReadOnlyList<string> PlanIds => _plans.Select(x => x.ID).ToList().AsReadOnly();
    public IReadOnlyList<IFarmerAlert> Alerts => _alerts.AsReadOnly();
    public IReadOnlyList<string> AlertIds => _alerts.Select(x => x.ID).ToList().AsReadOnly();

    public string IrrigationPlanId { get; set; }
    public bool CanIrrigationPlanStart { get; set; }
    
    public double WidthInMeters { get; set; }
    public double LengthInMeters { get; set; }

    public string[] GetPlantIds()
    {
        return plantIdsToResolve ?? PlantIds.ToArray();
    }

    public string[] GetPlanIds()
    {
        return planIdsToResolve ?? PlanIds.ToArray();
    }

    public string[] GetAlertIds()
    {
        return alertIdsToResolve ?? AlertIds.ToArray();
    }

    public async Task<bool> ExecutePlan(string planId, CancellationToken token)
    {
        if (!PlanIds.Contains(planId))
        {
            SmartFarmerLog.Error("Plan is not defined");

            await Task.CompletedTask;
            return false;
        }

        var plan = _plans.FirstOrDefault(x => x.ID == planId) as IFarmerRunningPlan;
        if (plan == null)
        {
            SmartFarmerLog.Error("Invalid plan");

            await Task.CompletedTask;
            return false;
        }

        _planExecSem.Wait();
        
        try 
        {
            await plan.Execute(token);
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            throw;
        }
        finally
        {
            _planExecSem.Release();
        }

        return true;
    }

    public void AddAlerts(List<IFarmerAlert> alerts)
    {
        alertIdsToResolve = 
            alertIdsToResolve
                .Except( alerts.Select(x => x.ID).ToArray() )
                .ToArray();

        _alerts.AddRange(alerts);
    }

    public void AddAlert(IFarmerAlert alert)
    {
        alertIdsToResolve = 
            alertIdsToResolve
                .Except( new [] {alert.ID} )
                .ToArray();

        _alerts.Add(alert);
    }

    public void AddAlert(string alertId)
    {
        throw new InvalidOperationException();
    }

    public void RemoveAlert(string alertId)
    {
        throw new InvalidOperationException();
    }

    public void AddPlans(List<IFarmerPlan> plans)
    {
        planIdsToResolve = 
            planIdsToResolve
                .Except( plans.Select(x => x.ID).ToArray() )
                .ToArray();

        plans
            .Where(x => x is FarmerPlan)
            .Cast<FarmerPlan>()
            .ToList()
            .ForEach(plan => plan.PropagateGarden(this));

        _plans.AddRange(plans);
    }

    public void AddPlan(string planId)
    {
        throw new InvalidOperationException();
    }

    public void RemovePlan(string planId)
    {
        throw new InvalidOperationException();
    }

    public void AddPlant(string plantId)
    {
        throw new InvalidOperationException();
    }

    public void AddPlants(List<IFarmerPlantInstance> plants)
    {
        plantIdsToResolve = 
            plantIdsToResolve
                .Except( plants.Select(x => x.ID).ToArray() )
                .ToArray();

        _plants.AddRange(plants);
    }

    public void AddPlants(string[] plantIds)
    {
        throw new InvalidOperationException();
    }

    public void RemovePlant(string plantId)
    {
        throw new InvalidOperationException();
    }

    public void MarkAlertAsRead(string alertId, bool read)
    {
        throw new InvalidOperationException();
    }
}