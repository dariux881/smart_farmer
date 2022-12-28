using System;
using System.Threading;
using System.Threading.Tasks;
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
        public Exception? LastException { get; set; }

        public async Task Execute(object[]? parameters, CancellationToken token)
        {
            if (Job == null) throw new ArgumentNullException(nameof(Job));

            await Task.Delay(Delay, token);

            var toolManager = FarmerToolsManager.Instance;
            var currentlyMountedTool = toolManager.GetCurrentlyMountedTool();

            if (Job.RequiredTool != Utils.FarmerTool.None && Job.RequiredTool != currentlyMountedTool)
            {
                await toolManager.MountTool(Job.RequiredTool);
            }

            await Job.Execute(parameters, token);
        }

    }
}
