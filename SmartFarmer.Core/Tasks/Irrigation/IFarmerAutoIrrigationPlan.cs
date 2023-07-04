using System;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Irrigation
{
    [Obsolete]
    public interface IFarmerAutoIrrigationPlan : IFarmerPlan
    {
        void AddIrrigationStep(IFarmerPlantInstance plant, FarmerIrrigationTaskInfo irrigationInfo);

        bool CanAutoIrrigationPlanStart { get; }
        DateTime PlannedAt { get; }
    }
}
