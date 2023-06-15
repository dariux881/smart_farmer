
using System;
using System.Collections.Generic;
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

        }

        public bool CanAutoIrrigationPlanStart { get; set; }

        public DateTime PlannedAt { get; set; }
    }
}
