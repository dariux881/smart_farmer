using System;
using System.Collections.Generic;
using System.Linq;
using SmartFarmer.Alerts;
using SmartFarmer.Plants;
using SmartFarmer.Settings;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Implementation;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer
{
    public class FarmerGround : IFarmerGround, IDisposable
    {
        private List<IFarmerPlantInstance> _plants;
        private FarmerAlertHandler _alertHandler;

        private string _userId;

        public FarmerGround()
        {
            _plants = new List<IFarmerPlantInstance>();
            Plans = new List<IFarmerPlan>();
            Alerts = new List<IFarmerAlert>();

            _alertHandler = FarmerAlertHandler.Instance;
            _alertHandler.NewAlertCreated += OnNewAlertReceived;
        }

        public FarmerGround(string userId)
            : this()
        {
            _userId = userId;
        }

#region Public Properties
        public string ID { get; }
        public string GroundName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public IReadOnlyList<IFarmerPlantInstance> Plants => _plants.AsReadOnly();
        public ICollection<IFarmerAlert> Alerts { get; private set; }
        public ICollection<IFarmerPlan> Plans { get; private set; }
        public IFarmerAutoIrrigationPlan GroundIrrigationPlan { get; private set; }

        public double WidthInMeters { get; set; }

        public double LengthInMeters { get; set; }

#endregion

#region Public Methods

        public void AddPlants(IFarmerPlantInstance[] plants)
        {
            foreach (var plant in plants)
            {
                _plants.Add(plant);
            }

            BuildAutoGroundIrrigationPlan();
        }

        public void AddPlant(IFarmerPlantInstance plant)
        {
            _plants.Add(plant);
            BuildAutoGroundIrrigationPlan();
        }

        
        public void Dispose()
        {
            if (_alertHandler != null)
            {
                _alertHandler.NewAlertCreated -= OnNewAlertReceived;
            }
        }

        public void RemovePlant(IFarmerPlantInstance plant)
        {
            _plants.Remove(plant);
            BuildAutoGroundIrrigationPlan();
        }
#endregion

#region Private Methods

        private void BuildAutoGroundIrrigationPlan()
        {
            // Steps:
            // - list all plants, minimizing movements
            var orderedPlants = 
                OrderPlantsToMinimizeMovements()
                    .ToList();

            var userSettings = 
                UserDefinedSettingsProvider
                    .GetUserDefinedSettings(_userId);

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
