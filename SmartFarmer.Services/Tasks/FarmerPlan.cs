﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Tasks
{

    public class FarmerPlan : IFarmerPlan
    {
        public FarmerPlan()
        {
            Steps = new List<IFarmerPlanStep>();
        }

        public IList<IFarmerPlanStep> Steps { get; private set; }
        public bool IsPlanInProgress { get; set; }

        public async Task Execute(CancellationToken token)
        {
            IsPlanInProgress = true;
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
                IsPlanInProgress = false;
            }
        }
    }
}
