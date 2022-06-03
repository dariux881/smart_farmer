
using SmartFarmer.Tasks;

namespace SmartFarmer.Plants
{
    public class FarmerPlant : IFarmerPlant
    {
        public string Name { get; set; }

        public IFarmerIrrigationTaskInfo IrrigationInfo { get; set; }

        public int MonthToPlan { get; set; }

        public int NumberOfWeeksToHarvest { get; set; }
    }
}
