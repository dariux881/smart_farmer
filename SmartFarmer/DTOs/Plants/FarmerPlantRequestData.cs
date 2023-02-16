
using System;

namespace SmartFarmer.DTOs.Plants;

public class FarmerPlantRequestData
{
    public string PlantKindID { get; set; }
    public string FarmerGroundId { get; set; }
    public int PlantX { get; set; }
    public int PlantY { get; set; }
    public int? PlantWidth { get; set; }
    public int? PlantDepth { get; set; }
    public DateTime? PlantedWhen { get; set; }
}