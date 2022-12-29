using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks
{

    public abstract class FarmerPlan : IFarmerPlan
    {
        protected FarmerPlan()
        {
            Name = StringUtils.RandomString(10);
            Steps = new List<IFarmerPlanStep>();
        }

        public FarmerPlan(string name) 
            : this() 
        {
            Name = name;
        }

        public string Name { get; private set; }
        public IList<IFarmerPlanStep> Steps { get; protected init; }
        public bool IsInProgress { get; private set; }
        public Exception LastException { get; private set; }

        public async Task Execute(CancellationToken token)
        {
            // resetting last exception, related to previous executions
            LastException = null;
            IsInProgress = true;

            // starting new plan execution
            SmartFarmerLog.Information("starting plan " + Name);
            
            try
            {
                foreach (var step in Steps)
                {
                    await step.Execute(null, token);
                }
            }
            catch (TaskCanceledException taskCanceled)
            {
                LastException = taskCanceled;
                SmartFarmerLog.Exception(taskCanceled);
            }
            catch (Exception ex)
            {
                LastException = ex;
                SmartFarmerLog.Exception(ex);
            }
            finally
            {
                IsInProgress = false;
                SmartFarmerLog.Information("stopping plan " + Name);
            }
        }
    }
}
