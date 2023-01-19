
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Utils;

namespace SmartFarmer.Plants
{
    public interface IFarmerPlant : IFarmerService
    {
        string Code { get; }
        string FriendlyName { get; }
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
