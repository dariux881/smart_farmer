
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.Plants
{
    public class FarmerPlant : IFarmerPlant
    {
        public string ID { get; set; }
        public string Code { get; set; }
        public string FriendlyName { get; set; }

        public IFarmerIrrigationTaskInfo IrrigationInfo { get; set; }
        public int PlantWidth { get; set; }
        public int PlantDepth { get; set; }
        public int MonthToPlan { get; set; }
        public int NumberOfWeeksToHarvest { get; set; }
    }
}
