using System;
using System.Collections.Generic;
using System.Linq;
using SmartFarmer.Data.Plants;
using SmartFarmer.Data.Tasks;
using SmartFarmer.Data.Alerts;
using Newtonsoft.Json;

namespace SmartFarmer.Data;

public class FarmerGround : IFarmerGround
{
    private string[] plantIdsToResolve;
    private string[] planIdsToResolve;
    private string[] alertIdsToResolve;

    public FarmerGround()
    {
        Alerts = new List<FarmerAlert>();
        Plants = new List<FarmerPlantInstance>();
        Plans = new List<FarmerPlan>();
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

    public List<FarmerPlantInstance> Plants { get; set; }
    public IReadOnlyList<string> PlantIds => Plants.Select(x => x.ID).ToList().AsReadOnly();

    public List<FarmerPlan> Plans { get; set; }
    public IReadOnlyList<string> PlanIds => Plans.Select(x => x.ID).ToList().AsReadOnly();

    public List<FarmerAlert> Alerts { get; set; }
    public IReadOnlyList<string> AlertIds => Alerts.Select(x => x.ID).ToList().AsReadOnly();

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

    public void AddAlert(string alertId)
    {
        throw new InvalidOperationException();
    }

    public void RemoveAlert(string alertId)
    {
        throw new InvalidOperationException();
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