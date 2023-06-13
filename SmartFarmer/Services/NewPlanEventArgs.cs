using System;

namespace SmartFarmer.Services;

public class PlanEventArgs : EventArgs
{
    public string PlanId { get; }
    public string FarmerGroundId { get; }

    public PlanEventArgs(string groundId, string planId)
    {
        FarmerGroundId = groundId;
        PlanId = planId;
    }
}