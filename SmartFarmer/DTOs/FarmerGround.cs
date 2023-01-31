
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
using SmartFarmer.Plants;

namespace SmartFarmer.DTOs;

public class FarmerGround : IFarmerGround
{
    private ConcurrentBag<string> _alerts;
    private ConcurrentBag<string> _plans;

    private SemaphoreSlim _plantsSem;
    private ISmartFarmerRepository _repository;

    public FarmerGround()
    {
        _alerts = new ConcurrentBag<string>();
        _plans = new ConcurrentBag<string>();

        _plantsSem = new SemaphoreSlim(5);
        Plants = new List<FarmerPlantInstance>();
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

    public IReadOnlyList<string> PlanIds => _alerts.ToList().AsReadOnly();
    public IReadOnlyList<string> AlertIds => _plans.ToList().AsReadOnly();
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
        if (string.IsNullOrEmpty(planId)) throw new ArgumentNullException(nameof(planId));

        _plans?.Add(planId);
    }

    public void RemovePlan(string planId)
    {
        if (string.IsNullOrEmpty(planId)) throw new ArgumentNullException(nameof(planId));
        if (_plans == null) return;
        
        _plans = new ConcurrentBag<string>(_plans.Except(new[] { planId }));
    }

    public void AddPlant(string plantId)
    {
        throw new InvalidOperationException();
    }

    public async Task AddPlantAsync(string plantId)
    {
        if (_repository == null) throw new InvalidOperationException("no repository provided");

        var plant = await _repository.GetPlantById(plantId) as FarmerPlantInstance;
        if (plant == null)
        {
            throw new InvalidCastException("plant is not a proper FarmerPlantInstance");
        }

        Plants?.Add(plant);
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