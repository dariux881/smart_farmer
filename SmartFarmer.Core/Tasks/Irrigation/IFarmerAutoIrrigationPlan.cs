using System;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Irrigation
{
    public interface IFarmerAutoIrrigationPlan : IFarmerPlan
    {
        bool CanAutoGroundIrrigationPlanStart { get; }
        DateTime PlannedAt { get; }
    }
}
