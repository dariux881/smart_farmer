
using System;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Utils;

public class FarmerPlanStepProvider : FarmerServiceLocalProviderBase<IFarmerPlanStep>, IFarmerPlanStepProvider
{
    private static readonly Lazy<FarmerPlanStepProvider> _instance = new(() => new FarmerPlanStepProvider());
    public static FarmerPlanStepProvider Instance => _instance.Value;

    public FarmerPlanStepProvider()
        : base("PlanStep_")
    {
    }
}