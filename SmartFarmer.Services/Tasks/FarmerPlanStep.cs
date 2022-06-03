using System;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks
{
    public class FarmerPlanStep : IFarmerPlanStep
    {
        public IFarmerTask Job { get; set; }

        public TimeSpan Delay { get; set; }
        public bool IsInProgress { get; set; }
        public Exception? LastException { get; set; }
    }
}
