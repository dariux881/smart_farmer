
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.Plants
{
    public interface IFarmerPlant
    {
        string Name { get; }
        IFarmerIrrigationTaskInfo IrrigationInfo { get; }

        /// <summary>
        /// values in cells, where cells size depend on ground
        /// </summary>
        int PlantWidth { get; }
        int PlantDepth { get; }

        /// <summary>
        /// 1-12 value
        /// </summary>
        int MonthToPlan { get; }

        int NumberOfWeeksToHarvest { get; }
    }
}
