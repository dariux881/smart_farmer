using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;

namespace SmartFarmer.Tasks.Weed
{
    public class FarmerWeedRemovalTask : FarmerBaseTask, IFarmerWeedRemovalTask
    {
        public FarmerWeedRemovalTask()
        {
            RequiredTool = Utils.FarmerTool.Weed;
        }

        public override async Task Execute(object[] parameters, CancellationToken token)
        {
            PrepareTask();

            await Task.Delay(5000);
            SmartFarmerLog.Information("Weed removed");

            EndTask();

            await Task.CompletedTask;
        }
    }
}
