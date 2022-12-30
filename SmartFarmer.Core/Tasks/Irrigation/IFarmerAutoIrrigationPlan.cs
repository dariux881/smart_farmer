using System;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Irrigation
{
    public interface IFarmerAutoIrrigationPlan : IFarmerPlan
    {
        void AddIrrigationStep(int x, int y, IFarmerIrrigationTaskInfo irrigationInfo);

        bool CanAutoGroundIrrigationPlanStart { get; }
        DateTime PlannedAt { get; }
    }
}
