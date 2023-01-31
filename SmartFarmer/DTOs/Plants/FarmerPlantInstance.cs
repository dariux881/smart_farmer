using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SmartFarmer.Plants;

namespace SmartFarmer.DTOs.Plants;

public class FarmerPlantInstance : IFarmerPlantInstance
{
    public string PlantName { get; set; }
    
    [JsonIgnore]
    public FarmerPlant Plant { get; set; }
    public string PlantKindID 
    { 
        get => PlantID; 
        set { PlantID = value; }
    }

    [JsonIgnore]
    public string PlantID { get; set; }
    [JsonIgnore]
    public string FarmerGroundId { get; set; }
    public int PlantX { get; set; }
    public int PlantY { get; set; }
    public int PlantWidth { get; set; }
    public int PlantDepth { get; set; }
    public DateTime PlantedWhen { get; set; }
    public DateTime? LastIrrigation { get; set; }

    // public List<DateTime> IrrigationHistory { get; set; }

    public string ID { get; set; }
}
