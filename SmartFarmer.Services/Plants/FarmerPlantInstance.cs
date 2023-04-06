using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Plants
{
    public class FarmerPlantInstance : IFarmerPlantInstance
    {
        private IFarmerPlant _plant;
        private string _plantKindId; 

        public FarmerPlantInstance(string ID, string PlantKindID, string PlantName)
        {
            if (PlantKindID == null) throw new ArgumentNullException(nameof(PlantKindID));

            this.ID = ID;
            this.PlantKindID = PlantKindID;
            this.PlantName = PlantName;

            IrrigationHistory = new List<DateTime>();
        }

        public string PlantKindID 
        {
            get => _plantKindId; 
            private set
            {
                _plantKindId = value;

                Task.Run(async () => await AssociatePlant());
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public IFarmerPlant Plant 
        {
            get => _plant;
            private set
            {
                _plant = value;
                this.PlantKindID = _plant?.ID;
            }
        }

        public string ID { get; set; }
        public string PlantName { get; set; }
        public int PlantX { get; set; }
        public int PlantY { get; set; }
        public int PlantWidth => Plant?.PlantWidth ?? 0;
        public int PlantDepth => Plant?.PlantDepth ?? 0;

        public DateTime PlantedWhen { get; set; }

        public DateTime? LastIrrigation => IrrigationHistory.LastOrDefault();

        public List<DateTime> IrrigationHistory { get; private set; }

        private async Task AssociatePlant()
        {
            if (string.IsNullOrEmpty(PlantKindID))
            {
                this.Plant = null;
                await Task.CompletedTask;
                return;
            }

            if (this.Plant == null)
            {
                this.Plant = await FarmerPlantProvider.Instance.GetFarmerService(PlantKindID);
                return;
            }

            var calculatedKind = await FarmerPlantProvider.Instance.GetFarmerService(this.Plant.ID);
            if (_plantKindId != calculatedKind?.ID)
            {
                Plant = calculatedKind;
            }
        }
    }
}
