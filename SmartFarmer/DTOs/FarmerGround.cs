using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using SmartFarmer.Data;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.DTOs.Security;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.DTOs.Alerts;

namespace SmartFarmer.DTOs;

public class FarmerGround : IFarmerGround
{
    private ISmartFarmerRepository _repository;

    public FarmerGround()
    {
        Alerts = new List<FarmerAlert>();
        Plants = new List<FarmerPlantInstance>();
        Plans = new List<FarmerPlan>();
    }

    public FarmerGround(ISmartFarmerRepository repository)
        : this()
    {
        _repository = repository;
    }

    public string ID { get; set; }
    public string GroundName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [JsonIgnore]
    public User User { get; set; }
    public string UserID { get; set; }

    [JsonIgnore]
    public List<FarmerPlantInstance> Plants { get; set; }
    public IReadOnlyList<string> PlantIds => Plants.Select(x => x.ID).ToList().AsReadOnly();

    [JsonIgnore]
    public List<FarmerPlan> Plans { get; set; }
    public IReadOnlyList<string> PlanIds => Plans.Select(x => x.ID).ToList().AsReadOnly();

    [JsonIgnore]
    public List<FarmerAlert> Alerts { get; set; }
    public IReadOnlyList<string> AlertIds => Alerts.Select(x => x.ID).ToList().AsReadOnly();

    public bool CanIrrigationPlanStart { get; set; }
    public string GroundIrrigationPlanId { get; set; }
    public double WidthInMeters { get; set; }
    public double LengthInMeters { get; set; }

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