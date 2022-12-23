using System;
using System.Collections.Generic;

namespace SmartFarmer.Plants
{
    public interface IFarmerPlantInstance
    {
        string PlantName { get; }
        IFarmerPlant Plant { get; }
        int PlantX { get; }
        int PlantY { get; }
        DateTime PlantedWhen { get; }
        DateTime? LastIrrigation { get; }
        IList<DateTime> IrrigationHistory { get; }
    }
}
