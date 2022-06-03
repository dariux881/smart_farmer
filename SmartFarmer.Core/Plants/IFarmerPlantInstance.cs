using SmartFarmer.Plants;
using System;
using System.Collections.Generic;

namespace SmartFarmer
{
    public interface IFarmerPlantInstance
    {
        string PlantName { get; }
        IFarmerPlant Plant { get; }
        DateTime PlantedWhen { get; }
        DateTime? LastIrrigation { get; }
        IList<DateTime> IrrigationHistory { get; }
    }
}
