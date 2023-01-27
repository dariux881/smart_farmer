using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.DTOs.Tasks;

public class FarmerIrrigationTaskInfo : IFarmerIrrigationTaskInfo
{
    public string ID { get; set; }
    public double AmountOfWaterInLitersPerTime { get; set; }

    public int TimesPerWeek { get; set; }
}