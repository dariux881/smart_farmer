
using System;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.Tasks
{
    public class FarmerAutoIrrigationPlan : FarmerPlan, IFarmerAutoIrrigationPlan
    {
        public FarmerAutoIrrigationPlan() 
            : base("AutoIrrigationPlan")
        {

        }

        public bool CanAutoGroundIrrigationPlanStart { get; set; }

        public DateTime PlannedAt { get; set; }
    }
}
