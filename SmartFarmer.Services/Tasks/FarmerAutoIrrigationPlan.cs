
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;
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
