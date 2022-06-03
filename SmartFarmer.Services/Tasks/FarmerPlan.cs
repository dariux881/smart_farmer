using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks
{

    public class FarmerPlan : IFarmerPlan
    {
        public FarmerPlan()
        {
            Steps = new List<IFarmerPlanStep>();
        }

        public IList<IFarmerPlanStep> Steps { get; private set; }
        public bool IsInProgress { get; set; }
        public Exception? LastException { get; set; }

        public async Task Execute(CancellationToken token)
        {
            IsInProgress = true;
            try
            {
                foreach (var step in Steps)
                {
                    var task = TaskExecutorCollectorService.Instance.GetExecutorByTool(step.Job.RequiredTool);
                    if (step.Job.RequiredTool != Utils.FarmerTool.None || task == null)
                    {
                        return;
                    }

                    await Task.Delay(step.Delay);
                    await task.Execute(null, token);
                }
            }
            catch (TaskCanceledException taskCanceled)
            {

            }
            finally
            {
                IsInProgress = false;
            }
        }
    }
}
