using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SmartFarmer.Alerts;
using SmartFarmer.Plants;
using SmartFarmer.Settings;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Implementation;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Utils;

namespace SmartFarmer;

public class FarmerGround : IFarmerGround, IDisposable
{
    private List<IFarmerPlantInstance> _plants;
    private List<IFarmerPlan> _plans;
    private List<IFarmerAlert> _alerts;
    private FarmerAlertHandler _alertHandler;
    private IFarmerPlantProvider _plantsProvider;
    private IFarmerPlantInstanceProvider _plantsInstanceProvider;
    private IFarmerPlanProvider _planProvider;
    private IFarmerAlertProvider _alertProvider;
    private bool _buildAutoIrrigationPlan;

    [JsonConstructor]
    public FarmerGround(
        string id,
        string groundName, 
        double latitude, 
        double longitude, 
        double widthInMeters,
        double lengthInMeters, 
        string groundIrrigationPlanId, 
        string userId, 
        string[] plantIds,
        string[] planIds,
        string[] alertIds,
        bool buildAutoIrrigationPlan = true)
        : this(
            id,
            groundName, 
            latitude, 
            longitude, 
            widthInMeters,
            lengthInMeters, 
            groundIrrigationPlanId,
            userId, 
            plantIds, 
            planIds, 
            alertIds,
            FarmerPlantProvider.Instance,
            FarmerPlantInstanceProvider.Instance, 
            FarmerPlanProvider.Instance, 
            FarmerAlertProvider.Instance, 
            FarmerAlertHandler.Instance,
            buildAutoIrrigationPlan)
    {

    }

    public FarmerGround(
        string id,
        string groundName, 
        double latitude, 
        double longitude, 
        double widthInMeters,
        double lengthInMeters, 
        string groundIrrigationPlanId, 
        string userId, 
        string[] plantIds,
        string[] planIds,
        string[] alertIds,
        IFarmerPlantProvider plantProvider,
        IFarmerPlantInstanceProvider plantInstanceProvider,
        IFarmerPlanProvider planProvider, 
        IFarmerAlertProvider alertProvider, 
        FarmerAlertHandler alertHandler,
        bool buildAutoIrrigationPlan = true)
        : this(plantProvider, plantInstanceProvider, planProvider, alertProvider, alertHandler, buildAutoIrrigationPlan)
    {
        ID = id;
        GroundName = groundName;
        Latitude = latitude;
        Longitude = longitude;
        WidthInMeters = widthInMeters;
        LengthInMeters = lengthInMeters;

        if (!string.IsNullOrEmpty(groundIrrigationPlanId))
        {
            GroundIrrigationPlan = planProvider.GetFarmerService(groundIrrigationPlanId) as IFarmerAutoIrrigationPlan;
        }

        _plants = plantIds?
                    .Select(x => plantInstanceProvider?.GetFarmerService(x))
                    .ToList()
                    ?? new List<IFarmerPlantInstance>();

        _plans = planIds?
                    .Select(x => planProvider?.GetFarmerService(x))
                    .ToList()
                    ?? new List<IFarmerPlan>();

        _alerts = alertIds?
                    .Select(x => alertProvider?.GetFarmerService(x))
                    .ToList()
                    ?? new List<IFarmerAlert>();

        UserID = userId;
    }

    public FarmerGround(
        IFarmerPlantProvider plantProvider,
        IFarmerPlantInstanceProvider plantInstanceProvider,
        IFarmerPlanProvider planProvider,
        IFarmerAlertProvider alertProvider, 
        FarmerAlertHandler alertHandler,
        bool buildAutoIrrigationPlan = true)
    {
        _plants = new List<IFarmerPlantInstance>();
        _plans = new List<IFarmerPlan>();
        _alerts = new List<IFarmerAlert>();

        _buildAutoIrrigationPlan = buildAutoIrrigationPlan;

        _plantsProvider = plantProvider;
        _plantsInstanceProvider = plantInstanceProvider;
        _planProvider = planProvider;
        _alertProvider = alertProvider;
        _alertHandler = alertHandler;

        if (_alertHandler != null)
        {
            _alertHandler.NewAlertCreated += OnNewAlertReceived;
        }
    }

    public event EventHandler<FarmerAlertHandlerEventArgs> NewAlertReceived;

#region Public Properties

    public string ID { get; private set; }
    public string GroundName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string UserID { get; private set; }

#region Plants
    
    public IReadOnlyList<string> PlantIds => 
        _plants?
            .Select(x => x.ID)
            .ToList()
            .AsReadOnly();
    
    [JsonIgnore]
    public IReadOnlyList<IFarmerPlantInstance> Plants => _plants.AsReadOnly();

#endregion

    public IReadOnlyList<string> AlertIds => 
        _alerts?
            .Select(x => x.ID)
            .ToList()
            .AsReadOnly();

    [JsonIgnore]
    public IReadOnlyList<IFarmerAlert> Alerts => _alerts.AsReadOnly();

#region Plans
    
    public IReadOnlyList<string> PlanIds => Plans?.Select(x => x.ID).ToList().AsReadOnly();
    
    [JsonIgnore]
    public IReadOnlyList<IFarmerPlan> Plans => _plans?.AsReadOnly();
    
    public string GroundIrrigationPlanId => GroundIrrigationPlan?.ID;
    
    [JsonIgnore]
    public IFarmerAutoIrrigationPlan GroundIrrigationPlan { get; private set; }

#endregion

    public double WidthInMeters { get; set; }

    public double LengthInMeters { get; set; }

#endregion

#region Public Methods

    public void AddPlants(string[] plantIds)
    {
        if (plantIds == null) throw new ArgumentNullException(nameof(plantIds));

        this.AddPlants(plantIds.Select(x => _plantsInstanceProvider.GetFarmerService(x)).ToArray());
    }

    public void AddPlant(string plant)
    {
        if (plant == null) throw new ArgumentNullException(nameof(plant));

        this.AddPlant(_plantsInstanceProvider.GetFarmerService(plant));
    }

    public void RemovePlant(string plant)
    {
        if (plant == null) throw new ArgumentNullException(nameof(plant));

        this.RemovePlant(_plantsInstanceProvider.GetFarmerService(plant));
    }

    public void AddPlan(string planId)
    {
        if (planId == null) throw new ArgumentNullException(nameof(planId));

        this.AddPlan(_planProvider.GetFarmerService(planId));
    }

    public void RemovePlan(string planId)
    {
        if (planId == null) throw new ArgumentNullException(nameof(planId));

        this.RemovePlan(_planProvider.GetFarmerService(planId));
    }


    public void AddAlert(string alertId)
    {
        if (string.IsNullOrEmpty(alertId)) throw new ArgumentNullException(nameof(alertId));

        AddAlert(_alertProvider?.GetFarmerService(alertId));
    }

    public void RemoveAlert(string alertId)
    {
        if (string.IsNullOrEmpty(alertId)) throw new ArgumentNullException(nameof(alertId));

        RemoveAlert(_alertProvider?.GetFarmerService(alertId));
    }

    public void MarkAlertAsRead(string alertId, bool read)
    {
        if (string.IsNullOrEmpty(alertId)) throw new ArgumentNullException(nameof(alertId));

        MarkAlertAsRead(_alertProvider?.GetFarmerService(alertId), read);
    }

    public void Dispose()
    {
        if (_alertHandler != null)
        {
            _alertHandler.NewAlertCreated -= OnNewAlertReceived;
        }
    }

#endregion

#region Private Methods

    private void BuildAutoGroundIrrigationPlan()
    {
        if (!_buildAutoIrrigationPlan)
        {
            return;
        }

        // Steps:
        // - list all plants, minimizing movements
        var orderedPlants = 
            OrderPlantsToMinimizeMovements()
                .ToList();

        var userSettings = 
            UserDefinedSettingsProvider
                .GetUserDefinedSettings(UserID);

        var oldGroundIrrigationPlanId = GroundIrrigationPlan?.ID;

        GroundIrrigationPlan = 
            new FarmerAutoIrrigationPlan(FarmerPlanProvider.Instance.GenerateServiceId())
            {
                CanAutoGroundIrrigationPlanStart = 
                    userSettings.AUTOIRRIGATION_AUTOSTART,
                PlannedAt = 
                    userSettings.AUTOIRRIGATION_PLANNED_TIME
            };

        FarmerPlanProvider.Instance.AddFarmerService(GroundIrrigationPlan);

        // if (oldGroundIrrigationPlanId == null)
        // {
        //     FarmerPlanProvider.Instance.RemoveFarmerService(oldGroundIrrigationPlanId);
        // }

        // asking irrigation
        orderedPlants.ForEach(plant => 
            {
                var plantKind = _plantsProvider.GetFarmerService(plant.PlantKindID);

                GroundIrrigationPlan.AddIrrigationStep(
                    plant.PlantX, 
                    plant.PlantY, 
                    plantKind.IrrigationInfo);
            });
    }

    private IReadOnlyCollection<IFarmerPlantInstance> OrderPlantsToMinimizeMovements()
    {
        //TODO implement sorting to minimize movements
        return Plants;
    }

    private IFarmerPlantInstance GetPlantInstanceById(string id)
    {
        return Plants.FirstOrDefault(x => x.ID == id);
    }

    private void AddPlants(IFarmerPlantInstance[] plants)
    {
        if (plants == null) throw new ArgumentNullException(nameof(plants));

        foreach (var plant in plants)
        {
            _plants.Add(plant);
        }

        BuildAutoGroundIrrigationPlan();
    }

    private void AddPlant(IFarmerPlantInstance plant)
    {
        if (plant == null) throw new ArgumentNullException(nameof(plant));

        _plants.Add(plant);
        BuildAutoGroundIrrigationPlan();
    }

    private void RemovePlant(IFarmerPlantInstance plant)
    {
        if (plant == null) throw new ArgumentNullException(nameof(plant));

        _plants.Remove(plant);
        BuildAutoGroundIrrigationPlan();
    }

    private void AddPlan(IFarmerPlan plan)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));

        _plans.Add(plan);
    }

    private void RemovePlan(IFarmerPlan plan)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));

        _plans.Remove(plan);
    }


#region Alerts 

    private void AddAlert(IFarmerAlert alert) 
    {
        if (alert == null) throw new ArgumentNullException(nameof(alert));
        _alerts.Add(alert);
    }

    private void RemoveAlert(IFarmerAlert alert) 
    {
        if (alert == null) throw new ArgumentNullException(nameof(alert));
        _alerts.Remove(alert);
    }

    private void MarkAlertAsRead(IFarmerAlert alert, bool read)
    {
        if (alert == null) throw new ArgumentNullException(nameof(alert));
        alert.MarkedAsRead = read;
    }

    private void OnNewAlertReceived(object sender, FarmerAlertHandlerEventArgs e)
    {
        AddAlert(e.AlertId);
        NewAlertReceived?.Invoke(sender, e);
    }

#endregion
#endregion
}
