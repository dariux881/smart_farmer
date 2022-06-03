
namespace SmartFarmer.Tasks
{
    public interface IFarmerIrrigationTaskInfo
    {
        double AmountOfWaterInLitersPerTime { get; }
        int TimesPerWeek { get; }
    }
}
