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

namespace SmartFarmer
{
    public class FarmerGround : IFarmerGround, IDisposable
    {
        private List<IFarmerPlantInstance> _plants;
        private List<IFarmerPlan> _plans;
        private FarmerAlertHandler _alertHandler;
        private FarmerPlantInstanceProvider _plantsProvider;
        private FarmerPlanProvider _planProvider;
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
                FarmerPlantInstanceProvider.Instance, 
                FarmerPlanProvider.Instance, 
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
            FarmerPlantInstanceProvider plantProvider,
            FarmerPlanProvider planProvider, 
            FarmerAlertHandler alertHandler,
            bool buildAutoIrrigationPlan = true)
            : this(plantProvider, planProvider, alertHandler, buildAutoIrrigationPlan)
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
                        .Select(x => plantProvider?.GetFarmerService(x))
                        .ToList()
                        ?? new List<IFarmerPlantInstance>();

            UserID = userId;
        }

        public FarmerGround(
            FarmerPlantInstanceProvider plantProvider,
            FarmerPlanProvider planProvider,
            FarmerAlertHandler alertHandler,
            bool buildAutoIrrigationPlan = true)
        {
            _plants = new List<IFarmerPlantInstance>();
            _plans = new List<IFarmerPlan>();
            Alerts = new List<IFarmerAlert>();

            _buildAutoIrrigationPlan = buildAutoIrrigationPlan;
            _plantsProvider = plantProvider;
            _planProvider = planProvider;
            _alertHandler = alertHandler;

            if (_alertHandler != null)
            {
                _alertHandler.NewAlertCreated += OnNewAlertReceived;
            }
        }

#region Public Properties

        public string ID { get; private set; }
        public string GroundName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string UserID { get; private set; }

#region Plants
        
        public IReadOnlyList<string> PlantIds => _plants?.Select(x => x.ID).ToList().AsReadOnly();
        
        [JsonIgnore]
        public IReadOnlyList<IFarmerPlantInstance> Plants => _plants.AsReadOnly();

#endregion

        public ICollection<IFarmerAlert> Alerts { get; private set; }

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

            this.AddPlants(plantIds.Select(x => _plantsProvider.GetFarmerService(x)).ToArray());
        }

        public void AddPlant(string plant)
        {
            if (plant == null) throw new ArgumentNullException(nameof(plant));

            this.AddPlant(_plantsProvider.GetFarmerService(plant));
        }

        public void RemovePlant(string plant)
        {
            if (plant == null) throw new ArgumentNullException(nameof(plant));

            this.RemovePlant(_plantsProvider.GetFarmerService(plant));
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

            GroundIrrigationPlan = 
                new FarmerAutoIrrigationPlan(FarmerPlanProvider.Instance.GenerateServiceId())
                {
                    CanAutoGroundIrrigationPlanStart = 
                        userSettings.AUTOIRRIGATION_AUTOSTART,
                    PlannedAt = 
                        userSettings.AUTOIRRIGATION_PLANNED_TIME
                };

            FarmerPlanProvider.Instance.AddFarmerService(GroundIrrigationPlan);

            // asking irrigation
            orderedPlants.ForEach(plant => 
                GroundIrrigationPlan.AddIrrigationStep(
                    plant.PlantX, 
                    plant.PlantY, 
                    plant.Plant.IrrigationInfo));
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

        public void AddPlan(IFarmerPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));

            _plans.Add(plan);
        }

        public void RemovePlan(IFarmerPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));

            _plans.Remove(plan);
        }


#region Alerts 

        private void AddAlert(IFarmerAlert alert) 
        {
            Alerts.Add(alert);
        }

        private void RemoveAlert(IFarmerAlert alert) 
        {
            Alerts.Remove(alert);
        }

        private void MarkAlertAsRead(IFarmerAlert alert, bool read)
        {
            alert.MarkedAsRead = read;
        }

        private IFarmerAlert GetAlertById(string id)
        {
            return Alerts.FirstOrDefault(x => x.ID == id);
        }

        private void OnNewAlertReceived(object sender, FarmerAlertHandlerEventArgs e)
        {
            AddAlert(e.Alert);
        }

#endregion
#endregion
    }
}
