
using SmartFarmer.Tasks;
using System.Collections.Generic;
using System.Linq;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Plants;

namespace SmartFarmer
{
    public class FarmerGround : IFarmerGround
    {
        private List<IFarmerPlantInstance> _plants;

        public FarmerGround()
        {
            _plants = new List<IFarmerPlantInstance>();
            Plans = new List<IFarmerPlan>();
        }

        public IReadOnlyList<IFarmerPlantInstance> Plants => _plants.AsReadOnly();
        public ICollection<IFarmerPlan> Plans { get; private set; }
        public IFarmerAutoIrrigationPlan GroundIrrigationPlan { get; private set; }

        public double WidthInMeters { get; set; }

        public double LengthInMeters { get; set; }

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

        public void RemovePlant(IFarmerPlantInstance plant)
        {
            _plants.Remove(plant);
            BuildAutoGroundIrrigationPlan();
        }

        private void BuildAutoGroundIrrigationPlan()
        {
            //TODO build plan considering plant position
        }
    }
}
