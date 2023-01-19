
using System;
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

        public void AddIrrigationStep(int x, int y, IFarmerIrrigationTaskInfo irrigationInfo)
        {
            //TODO evaluate if water is needed based on irrigationInfo.
            // If water is not needed, then return, avoiding adding useless tasks

            this.EditableSteps.Add(
                new FarmerPlanStep(
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerMoveOnGridTask)),
                    new object[] {x, y}));

            this.EditableSteps.Add(
                new FarmerPlanStep(
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerProvideWaterTask)),
                    new object[] {irrigationInfo.AmountOfWaterInLitersPerTime}));
        }

        public bool CanAutoGroundIrrigationPlanStart { get; set; }

        public DateTime PlannedAt { get; set; }
    }
}
