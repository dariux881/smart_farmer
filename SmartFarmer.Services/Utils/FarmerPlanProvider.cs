
using System;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Utils;

public class FarmerPlanProvider : FarmerServiceLocalProviderBase<IFarmerPlan>, IFarmerPlanProvider
{
    private static readonly Lazy<FarmerPlanProvider> _instance = new(() => new FarmerPlanProvider());
    public static FarmerPlanProvider Instance => _instance.Value;

    public FarmerPlanProvider()
        : base("Plan_")
    {
    }
}