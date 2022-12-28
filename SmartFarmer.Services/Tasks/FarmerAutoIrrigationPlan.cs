
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.Tasks
{
    public class FarmerAutoIrrigationPlan : IFarmerAutoIrrigationPlan
    {
        public bool CanAutoGroundIrrigationPlanStart => throw new NotImplementedException();

        public DateTime PlannedAt => throw new NotImplementedException();

        public IList<IFarmerPlanStep> Steps => throw new NotImplementedException();

        public bool IsInProgress => throw new NotImplementedException();

        public Exception? LastException => throw new NotImplementedException();

        public Task Execute(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
