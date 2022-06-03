using System;

namespace SmartFarmer.Tasks
{
    public class FarmerPlanStep : IFarmerPlanStep
    {
        public IFarmerTask Job { get; set; }

        public TimeSpan Delay { get; set; }
    }
}
