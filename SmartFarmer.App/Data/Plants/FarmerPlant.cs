using Newtonsoft.Json;
using SmartFarmer.Data.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.Data.Plants;

public class FarmerPlant : IFarmerPlant
{
    public FarmerPlant()
    {

    }
    
    [JsonConstructor]
    public FarmerPlant(string irrigationTaskInfoSerialized)
        : this()
    {
        IrrigationTaskInfo = irrigationTaskInfoSerialized?.Deserialize<FarmerIrrigationTaskInfo>();
    }

    public string BotanicalName { get; set; }

    public string FriendlyName { get; set; }

    public IFarmerIrrigationTaskInfo IrrigationTaskInfo { get; set; }

    public int PlantWidth { get; set; }

    public int PlantDepth { get; set; }

    public int MonthToPlan { get; set; }

    public int NumberOfWeeksToHarvest { get; set; }

    public string ID { get; set; }
}
