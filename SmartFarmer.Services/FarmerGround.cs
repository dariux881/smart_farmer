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
        private FarmerAlertHandler _alertHandler;
        private FarmerPlantInstanceProvider _plantsProvider;
        private bool _buildAutoIrrigationPlan;

        [JsonConstructor]
        public FarmerGround(
            string id,
            string groundName, 
            double latitude, 
            double longitude, 
            double widthInMeters,
            double lengthInMeters, 
            string userId, 
            string[] plantIds,
            bool buildAutoIrrigationPlan = true)
            : this(
                id,
                groundName, 
                latitude, 
                longitude, 
                widthInMeters,
                lengthInMeters, 
                userId, 
                plantIds, 
                FarmerPlantInstanceProvider.Instance, 
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
            string userId, 
            string[] plantIds,
            FarmerPlantInstanceProvider plantProvider,
            FarmerAlertHandler alertHandler,
            bool buildAutoIrrigationPlan = true)
            : this(plantProvider, alertHandler, buildAutoIrrigationPlan)
        {
            ID = id;
            GroundName = groundName;
            Latitude = latitude;
            Longitude = longitude;
            WidthInMeters = widthInMeters;
            LengthInMeters = lengthInMeters;
            _plants = plantIds?
                        .Select(x => plantProvider?.GetFarmerPlantInstance(x))
                        .ToList()
                        ?? new List<IFarmerPlantInstance>();

            UserID = userId;
        }

        public FarmerGround(
            FarmerPlantInstanceProvider plantProvider,
            FarmerAlertHandler alertHandler,
            bool buildAutoIrrigationPlan = true)
        {
            _plants = new List<IFarmerPlantInstance>();
            Plans = new List<IFarmerPlan>();
            Alerts = new List<IFarmerAlert>();

            _buildAutoIrrigationPlan = buildAutoIrrigationPlan;
            _plantsProvider = plantProvider;
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

        public IReadOnlyList<string> PlantIds => _plants?.Select(x => x.ID).ToList().AsReadOnly();

        [JsonIgnore]
        public IReadOnlyList<IFarmerPlantInstance> Plants => _plants.AsReadOnly();
        public ICollection<IFarmerAlert> Alerts { get; private set; }
        public ICollection<IFarmerPlan> Plans { get; private set; }
        public IFarmerAutoIrrigationPlan GroundIrrigationPlan { get; private set; }

        public double WidthInMeters { get; set; }

        public double LengthInMeters { get; set; }

#endregion

#region Public Methods

        public void AddPlants(string[] plantIds)
        {
            if (plantIds == null) throw new ArgumentNullException(nameof(plantIds));

            this.AddPlants(plantIds.Select(x => _plantsProvider.GetFarmerPlantInstance(x)).ToArray());
        }

        public void AddPlants(IFarmerPlantInstance[] plants)
        {
            if (plants == null) throw new ArgumentNullException(nameof(plants));

            foreach (var plant in plants)
            {
                _plants.Add(plant);
            }

            BuildAutoGroundIrrigationPlan();
        }

        public void AddPlant(string plant)
        {
            if (plant == null) throw new ArgumentNullException(nameof(plant));

            this.AddPlant(_plantsProvider.GetFarmerPlantInstance(plant));
        }

        public void AddPlant(IFarmerPlantInstance plant)
        {
            if (plant == null) throw new ArgumentNullException(nameof(plant));

            _plants.Add(plant);
            BuildAutoGroundIrrigationPlan();
        }

        public void RemovePlant(string plant)
        {
            if (plant == null) throw new ArgumentNullException(nameof(plant));

            this.RemovePlant(_plantsProvider.GetFarmerPlantInstance(plant));
        }

        public void RemovePlant(IFarmerPlantInstance plant)
        {
            if (plant == null) throw new ArgumentNullException(nameof(plant));

            _plants.Remove(plant);
            BuildAutoGroundIrrigationPlan();
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
                new FarmerAutoIrrigationPlan()
                {
                    CanAutoGroundIrrigationPlanStart = 
                        userSettings.AUTOIRRIGATION_AUTOSTART,
                    PlannedAt = 
                        userSettings.AUTOIRRIGATION_PLANNED_TIME
                };

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
