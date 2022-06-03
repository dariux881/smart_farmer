
namespace SmartFarmer.Tasks
{
    public class FarmerIrrigationTaskInfo : IFarmerIrrigationTaskInfo
    {
        public double AmountOfWaterInLitersPerTime { get; set; }

        public int TimesPerWeek { get; set; }
    }
}
