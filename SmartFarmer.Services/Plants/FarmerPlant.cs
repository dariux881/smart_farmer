
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.Plants
{
    public class FarmerPlant : IFarmerPlant
    {
        public FarmerPlant(string code, string friendlyName, IFarmerIrrigationTaskInfo irrigationInfo)
            : this("Plant_" + Extensions.RandomString(5), code, friendlyName, irrigationInfo)
        {
        }

        public FarmerPlant(string id, string code, string friendlyName, IFarmerIrrigationTaskInfo irrigationInfo)
        {
            ID = id;
            BotanicalName = code;
            FriendlyName = friendlyName;
            IrrigationTaskInfo = irrigationInfo;
        }

        public string ID { get; set; }
        public string BotanicalName { get; set; }
        public string FriendlyName { get; set; }

        public string IrrigationInfoId => IrrigationTaskInfo?.ID;
        public IFarmerIrrigationTaskInfo IrrigationTaskInfo { get; set; }
        public int PlantWidth { get; set; }
        public int PlantDepth { get; set; }
        public int MonthToPlan { get; set; }
        public int NumberOfWeeksToHarvest { get; set; }
    }
}
