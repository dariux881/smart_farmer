using System;

namespace SmartFarmer.Tasks
{
    public interface IFarmerPlanStep
    {
        IFarmerTask Job { get; }
        TimeSpan Delay { get; }
    }
}
