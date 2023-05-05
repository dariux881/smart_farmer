using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.PlantUtils
{
    public class FarmerFlowerDetector : FarmerBaseTask, IFarmerFlowerDetectorTask
    {
        public FarmerFlowerDetector()
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