using System;

namespace SmartFarmer.Services;

public class NewPlanEventArgs : EventArgs
{
    public string PlanId { get; }
    public string FarmerGroundId { get; }

    public NewPlanEventArgs(string groundId, string planId)
    {
        FarmerGroundId = groundId;
        PlanId = planId;
    }
}