
namespace SmartFarmer.Tasks.Irrigation
{
    public interface IFarmerIrrigationTaskInfo
    {
        double AmountOfWaterInLitersPerTime { get; }
        int TimesPerWeek { get; }
    }
}
