using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Irrigation
{
    public class FarmerProvideWaterTask : FarmerBaseTask, IFarmerProvideWaterTask
    {
        public FarmerProvideWaterTask()
        {
            RequiredTool = FarmerTool.Water;
        }

        public override async Task Execute(object[] parameters, CancellationToken token)
        {
            if (parameters == null || parameters.Length < 1) throw new ArgumentException(nameof(parameters));

            //TODO implement parsing by string for time span
            var amount = (double)parameters[0];

            await ProvideWater(amount, token);
        }

        public async Task ProvideWater(double amountInLiters, CancellationToken token)
        {
            IsInProgress = true;

            SmartFarmerLog.Debug($"providing {amountInLiters} liters of water");

            await Task.Delay(1000);

            SmartFarmerLog.Debug($"- done");
            IsInProgress = false;

            await Task.CompletedTask;
        }

        public async Task ProvideWater(TimeSpan span, CancellationToken token)
        {
            IsInProgress = true;

            SmartFarmerLog.Debug($"providing water for {span.TotalSeconds} seconds");

            await Task.Delay(span);

            SmartFarmerLog.Debug($"- done");
            IsInProgress = false;

            await Task.CompletedTask;
        }
    }
}