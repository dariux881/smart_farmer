using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;

namespace SmartFarmer.Tasks.PlantUtils
{
    public class FarmerLeafDetector : FarmerBaseTask, IFarmerLeafDetector
    {
        public FarmerLeafDetector()
        {
            RequiredTool = Utils.FarmerTool.Camera;
        }

        public override async Task Execute(object[] parameters, CancellationToken token)
        {
            PrepareTask();

            await Task.Delay(500);
            SmartFarmerLog.Information("Task completed");

            EndTask();

            await Task.CompletedTask;
        }
    }
}