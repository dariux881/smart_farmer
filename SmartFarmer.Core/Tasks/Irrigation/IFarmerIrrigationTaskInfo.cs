
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Irrigation
{
    public interface IFarmerIrrigationTaskInfo : IFarmerService
    {
        double AmountOfWaterInLitersPerTime { get; }
        int TimesPerWeek { get; }
    }
}
