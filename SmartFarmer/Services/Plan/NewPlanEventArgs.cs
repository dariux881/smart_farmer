using System;

namespace SmartFarmer.Services.Plan;

public class PlanEventArgs : EventArgs
{
    public string PlanId { get; }
    public string GardenId { get; }

    public PlanEventArgs(string gardenId, string planId)
    {
        GardenId = gardenId;
        PlanId = planId;
    }
}