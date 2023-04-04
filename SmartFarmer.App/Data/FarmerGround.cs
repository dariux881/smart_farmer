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

namespace SmartFarmer.Data;

public class FarmerGround : IFarmerGround
{
    private string[] plantIdsToResolve;
    private string[] planIdsToResolve;
    private string[] alertIdsToResolve;

    private List<IFarmerAlert> _alerts;
    private List<IFarmerPlantInstance> _plants;
    private List<IFarmerPlan> _plans;

    public FarmerGround()
    {
        _alerts = new List<IFarmerAlert>();
        _plants = new List<IFarmerPlantInstance>();
        _plans = new List<IFarmerPlan>();
    }

    [JsonConstructor]
    public FarmerGround(        
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
    public string GroundName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string UserID { get; set; }

    public IReadOnlyList<IFarmerPlantInstance> Plants => _plants.AsReadOnly();
    public IReadOnlyList<string> PlantIds => _plants.Select(x => x.ID).ToList().AsReadOnly();
    public IReadOnlyList<IFarmerPlan> Plans => _plans.AsReadOnly();
    public IReadOnlyList<string> PlanIds => _plans.Select(x => x.ID).ToList().AsReadOnly();
    public IReadOnlyList<IFarmerAlert> Alerts => _alerts.AsReadOnly();
    public IReadOnlyList<string> AlertIds => _alerts.Select(x => x.ID).ToList().AsReadOnly();

    public string GroundIrrigationPlanId { get; set; }
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

    public void AddAlerts(List<IFarmerAlert> alerts)
    {
        alertIdsToResolve = 
            alertIdsToResolve
                .Except( alerts.Select(x => x.ID).ToArray() )
                .ToArray();

        _alerts.AddRange(alerts);
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