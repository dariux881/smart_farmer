using System.Text.Json;
using System.Text.Json.Serialization;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.DTOs.Plants;

public class FarmerPlant : IFarmerPlant
{
    private IFarmerIrrigationTaskInfo _irrigationTaskInfo;

    public string Code { get; set; }

    public string FriendlyName { get; set; }

    [JsonIgnore]
    public IFarmerIrrigationTaskInfo IrrigationTaskInfo 
    { 
        get => _irrigationTaskInfo;
        set {
            _irrigationTaskInfo = value;
            SerializeParameters();
        }
    }

    [JsonIgnore]
    public string IrrigationTaskInfoSerialized { get; set; }

    public IFarmerIrrigationTaskInfo IrrigationTaskInfoString 
    { 
        get {
            if (string.IsNullOrEmpty(IrrigationTaskInfoSerialized))
            {
                return null;
            }

            return JsonSerializer.Deserialize<FarmerIrrigationTaskInfo>(IrrigationTaskInfoSerialized);
        }
    }

    public int PlantWidth { get; set; }

    public int PlantDepth { get; set; }

    public int MonthToPlan { get; set; }

    public int NumberOfWeeksToHarvest { get; set; }

    public string ID { get; set; }
    
    private void SerializeParameters()
    {
        if (IrrigationTaskInfo == null) {
            IrrigationTaskInfoSerialized = null;
            return;
        }

        IrrigationTaskInfoSerialized = 
            JsonSerializer
                .Serialize(IrrigationTaskInfo);
    }
}
