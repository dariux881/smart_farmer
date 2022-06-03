using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFarmer.Plants
{
    public class FarmerPlantInstance : IFarmerPlantInstance
    {
        public FarmerPlantInstance()
        {
            IrrigationHistory = new List<DateTime>();
        }

        public IFarmerPlant Plant { get; set; }

        public string PlantName { get; set; }
        public DateTime PlantedWhen { get; set; }

        public DateTime? LastIrrigation => IrrigationHistory.LastOrDefault();

        public IList<DateTime> IrrigationHistory { get; private set; }

    }
}
