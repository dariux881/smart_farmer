using System;
using SmartFarmer.Plants;

namespace SmartFarmer.Data.Plants;

public class FarmerPlantInstance : IFarmerPlantInstance
{
    private string _plantKindIdToResolve;
    private FarmerPlant _plant;

    public string PlantName { get; set; }
    public FarmerPlant Plant 
    {
        get => _plant; 
        set
        {
            if (value != null)
            {
                _plantKindIdToResolve = null;
            }

            _plant = value;
        }
    }

    public string PlantKindID
    {
        get => Plant?.ID ?? _plantKindIdToResolve;
        set
        {
            _plantKindIdToResolve = value;
        }
    }

    public int PlantX { get; set; }
    public int PlantY { get; set; }
    public int PlantWidth { get; set; }
    public int PlantDepth { get; set; }
    public DateTime PlantedWhen { get; set; }
    public DateTime? LastIrrigation { get; set; }

    // public List<DateTime> IrrigationHistory { get; set; }

    public string ID { get; set; }
}
