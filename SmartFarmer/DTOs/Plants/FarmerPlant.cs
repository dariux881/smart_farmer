using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.DTOs.Plants;

public class FarmerPlant : IFarmerPlant
{
    private string _irrigationTaskInfoSerialized;

    public string BotanicalName { get; set; }

    public string FriendlyName { get; set; }

    public FarmerIrrigationTaskInfo IrrigationTaskInfo 
    { 
        get => DeserializeIrrigationInfo(IrrigationTaskInfoSerialized);
        set {
            SetIrrigationInfo(value);
        }
    }

    [JsonIgnore]
    public string IrrigationTaskInfoSerialized 
    { 
        get => _irrigationTaskInfoSerialized;
        set
        {
            _irrigationTaskInfoSerialized = value;
        }
    }

    public int PlantWidth { get; set; }

    public int PlantDepth { get; set; }

    public int MonthToPlan { get; set; }

    public int NumberOfWeeksToHarvest { get; set; }

    public string ID { get; set; }
    
    private void SetIrrigationInfo(FarmerIrrigationTaskInfo value)
    {
        IrrigationTaskInfoSerialized = SerializeIrrigationInfo(value);
    }

    private string SerializeIrrigationInfo(FarmerIrrigationTaskInfo info)
    {
        if (info == null) {
            return null;
        }

        return JsonSerializer.Serialize(info);
    }

    private FarmerIrrigationTaskInfo DeserializeIrrigationInfo(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<FarmerIrrigationTaskInfo>(json);
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return null;
        }
    }
}
