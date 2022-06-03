using System;

namespace SmartFarmer
{
    public interface IFarmerPlanStep
    {
        IFarmerTask Job { get; }
        TimeSpan Delay { get; }
    }
}
