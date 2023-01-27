
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.Plants
{
    public class FarmerPlant : IFarmerPlant
    {
        public FarmerPlant(string code, string friendlyName, IFarmerIrrigationTaskInfo irrigationInfo)
            : this("Plant_" + StringUtils.RandomString(5), code, friendlyName, irrigationInfo)
        {
        }

        public FarmerPlant(string id, string code, string friendlyName, IFarmerIrrigationTaskInfo irrigationInfo)
        {
            ID = id;
            Code = code;
            FriendlyName = friendlyName;
            IrrigationInfo = irrigationInfo;
        }

        public string ID { get; set; }
        public string Code { get; set; }
        public string FriendlyName { get; set; }

        public string IrrigationInfoId => IrrigationInfo?.ID;
        public IFarmerIrrigationTaskInfo IrrigationInfo { get; set; }
        public int PlantWidth { get; set; }
        public int PlantDepth { get; set; }
        public int MonthToPlan { get; set; }
        public int NumberOfWeeksToHarvest { get; set; }
    }
}
