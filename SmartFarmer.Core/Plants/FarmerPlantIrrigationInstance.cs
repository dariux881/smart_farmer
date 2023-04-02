
using System;

namespace SmartFarmer.Plants;

public class FarmerPlantIrrigationInstance
{
    public string PlantId { get; set; }
    public DateTime? When { get; set; }
    public double? AmountInLiters { get; set; }
}