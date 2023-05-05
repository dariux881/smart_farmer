
using System;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Implementation
{
    public class FarmerAutoIrrigationPlan : FarmerPlan, IFarmerAutoIrrigationPlan
    {
        public FarmerAutoIrrigationPlan(string id) 
            : base(id, "AutoIrrigationPlan")
        {

        }

        public void AddIrrigationStep(IFarmerPlantInstance plant, IFarmerIrrigationTaskInfo irrigationInfo)
        {
            if (plant == null) throw new ArgumentNullException(nameof(plant));
            if (irrigationInfo == null) throw new ArgumentNullException(nameof(irrigationInfo));

            this.EditableSteps.Add(
                new FarmerPlanStep(
                    ID + "_1",
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerMoveOnGridTask)),
                    new object[] {plant.PlantX, plant.PlantY}));

            // If water is not needed, then return, avoiding adding useless tasks
            this.EditableSteps.Add(
                new FarmerPlanStep(
                    ID + "_2",
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerCheckIfWaterIsNeededTask)),
                    new object[] {plant.ID}));

            this.EditableSteps.Add(
                new FarmerPlanStep(
                    ID + "_3",
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerProvideWaterTask)),
                    new object[] {irrigationInfo.AmountOfWaterInLitersPerTime}));
        }

        public bool CanAutoGroundIrrigationPlanStart { get; set; }

        public DateTime PlannedAt { get; set; }
    }
}
