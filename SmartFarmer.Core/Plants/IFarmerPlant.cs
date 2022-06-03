
using SmartFarmer.Tasks;

namespace SmartFarmer.Plants
{
    public interface IFarmerPlant
    {
        string Name { get; }
        IFarmerIrrigationTaskInfo IrrigationInfo { get; }
        
        /// <summary>
        /// 1-12 value
        /// </summary>
        int MonthToPlan { get; }

        int NumberOfWeeksToHarvest { get; }
    }
}
