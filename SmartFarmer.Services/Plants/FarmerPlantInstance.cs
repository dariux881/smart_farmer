using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFarmer.Plants
{
    public class FarmerPlantInstance : IFarmerPlantInstance
    {
        public FarmerPlantInstance(IFarmerPlant plant)
        {
            if (plant == null) throw new ArgumentNullException(nameof(plant));

            IrrigationHistory = new List<DateTime>();

            this.Plant = plant;
            this.PlantName = plant.Name;
        }

        public IFarmerPlant Plant { get; }

        public string PlantName { get; set; }
        public int PlantX { get; set; }
        public int PlantY { get; set; }
        public DateTime PlantedWhen { get; set; }

        public DateTime? LastIrrigation => IrrigationHistory.LastOrDefault();

        public IList<DateTime> IrrigationHistory { get; private set; }

    }
}
