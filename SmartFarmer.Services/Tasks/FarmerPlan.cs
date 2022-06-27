using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks
{

    public abstract class FarmerPlan : IFarmerPlan
    {
        protected FarmerPlan()
        {
        }

        public IList<IFarmerPlanStep> Steps { get; protected init; }
        public bool IsInProgress { get; private set; }
        public Exception? LastException { get; private set; }

        public async Task Execute(CancellationToken token)
        {
            IsInProgress = true;
            try
            {
                foreach (var step in Steps)
                {
                    await step.Execute(null, token);
                }
            }
            catch (TaskCanceledException taskCanceled)
            {

            }
            catch (Exception ex)
            {
                LastException = ex;
            }
            finally
            {
                IsInProgress = false;
            }
        }
    }
}
