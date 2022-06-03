using System;

namespace SmartFarmer.Tasks
{
    public interface IFarmerAutoIrrigationPlan : IFarmerPlan
    {
        bool CanAutoGroundIrrigationPlanStart { get; }
        DateTime PlannedAt { get; }
    }
}
