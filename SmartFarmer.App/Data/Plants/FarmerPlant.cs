using SmartFarmer.Data.Tasks;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.Data.Plants;

public class FarmerPlant : IFarmerPlant
{
    public string BotanicalName { get; set; }

    public string FriendlyName { get; set; }

    public FarmerIrrigationTaskInfo IrrigationTaskInfo { get; set; }

    public int PlantWidth { get; set; }

    public int PlantDepth { get; set; }

    public int MonthToPlan { get; set; }

    public int NumberOfWeeksToHarvest { get; set; }

    public string ID { get; set; }
}
