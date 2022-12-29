using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks
{
    public class FarmerPlanStep : IFarmerPlanStep
    {
        public FarmerPlanStep(IFarmerTask job)
        {
            Job = job;
        }

        public IFarmerTask Job { get; private set; }

        public TimeSpan Delay { get; set; }
        public bool IsInProgress { get; set; }
        public Exception LastException { get; set; }

        public async Task Execute(object[] parameters, CancellationToken token)
        {
            if (Job == null) throw new ArgumentNullException(nameof(Job));

            await Task.Delay(Delay, token);

            var toolManager = FarmerToolsManager.Instance;
            var currentlyMountedTool = toolManager.GetCurrentlyMountedTool();

            if (Job.RequiredTool != Utils.FarmerTool.None && Job.RequiredTool != currentlyMountedTool)
            {
                // this task requires a tool that is not currently mounted. Mounting tool first. 
                // Exceptions may arise. Exceptions will stop next executions
                SmartFarmerLog.Information("mounting tool " + Job.RequiredTool);
                await toolManager.MountTool(Job.RequiredTool);
            }
            else
            {
                var message = 
                    Job.RequiredTool != Utils.FarmerTool.None ?
                        Job.RequiredTool + " already mounted" :
                        "this task does not require any tool";

                SmartFarmerLog.Debug(message);
            }

            SmartFarmerLog.Information("executing task " + Job.GetType().FullName);
            await Job.Execute(parameters, token);
        }

    }
}
