
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using SmartFarmer.Data;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.DTOs.Security;
using SmartFarmer.DTOs.Tasks;

namespace SmartFarmer.DTOs;

public class FarmerGround : IFarmerGround
{
    private ConcurrentBag<string> _alerts;
    //private ConcurrentBag<string> _plans;

    private SemaphoreSlim _plantsSem;
    private SemaphoreSlim _plansSem;
    private ISmartFarmerRepository _repository;

    public FarmerGround()
    {
        _alerts = new ConcurrentBag<string>();

        _plantsSem = new SemaphoreSlim(5);
        _plansSem = new SemaphoreSlim(5);

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

    public IReadOnlyList<string> AlertIds => _alerts.ToList().AsReadOnly();
    public string GroundIrrigationPlanId { get; set; }
    public double WidthInMeters { get; set; }
    public double LengthInMeters { get; set; }

    public void AddAlert(string alertId)
    {
        if (string.IsNullOrEmpty(alertId)) throw new ArgumentNullException(nameof(alertId));

        _alerts?.Add(alertId);
    }

    public void RemoveAlert(string alertId)
    {
        if (string.IsNullOrEmpty(alertId)) throw new ArgumentNullException(nameof(alertId));
        if (_alerts == null) return;
        
        _alerts = new ConcurrentBag<string>(_alerts.Except(new[] { alertId }));
    }


    public void AddPlan(string planId)
    {
        throw new InvalidOperationException();
    }

    public void RemovePlan(string planId)
    {
        if (string.IsNullOrEmpty(planId)) throw new ArgumentNullException(nameof(planId));

        _plansSem.Wait();
        if (Plans == null) {
            _plansSem.Release();
            return;
        }
        
        var plan = Plans.FirstOrDefault(x => x.ID == planId);
        if (plan == null) {
            _plansSem.Release();
            return;
        }

        Plans.Remove(plan);
        _plansSem.Release();
    }

    public void AddPlan(FarmerPlan plan)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));

        _plansSem.Wait();

        Plans?.Add(plan);

        _plansSem.Release();
    }

    public void AddPlant(string plantId)
    {
        throw new InvalidOperationException();
    }

    public void AddPlant(FarmerPlantInstance plant)
    {
        if (plant == null) throw new ArgumentNullException(nameof(plant));

        _plantsSem.Wait();

        Plants?.Add(plant);

        _plantsSem.Release();
    }

    public void AddPlants(string[] plantIds)
    {
        if (plantIds == null) throw new ArgumentNullException(nameof(plantIds));

        foreach(var plantId in plantIds)
        {
            AddPlant(plantId);
        }
    }

    public void AddPlants(FarmerPlantInstance[] plants)
    {
        if (plants == null) throw new ArgumentNullException(nameof(plants));

        foreach(var plant in plants)
        {
            AddPlant(plant);
        }
    }

    public void RemovePlant(string plantId)
    {
        if (string.IsNullOrEmpty(plantId)) throw new ArgumentNullException(nameof(plantId));

        _plantsSem.Wait();
        if (Plants == null) {
            _plantsSem.Release();
            return;
        }
        
        var plant = Plants.FirstOrDefault(x => x.ID == plantId);
        if (plant == null) {
            _plantsSem.Release();
            return;
        }

        Plants.Remove(plant);
        _plantsSem.Release();
    }

    public void MarkAlertAsRead(string alertId, bool read)
    {
        throw new NotImplementedException();
    }
}