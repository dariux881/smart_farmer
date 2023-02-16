using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Helpers;
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

            double amount = 0.0;
            bool result = false;

            if (parameters[0] is string)
            {
                result = double.TryParse(parameters[0] as string, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
            }
            else if (parameters[0].IsNumber())
            {
                amount = (double)parameters[0];
            }

            await ProvideWater(amount, token);
        }

        public async Task ProvideWater(double amountInLiters, CancellationToken token)
        {
            PrepareTask();

            SmartFarmerLog.Debug($"providing {amountInLiters} liters of water");
            await Task.Delay(1000);

            EndTask();
            await Task.CompletedTask;
        }

        public async Task ProvideWater(TimeSpan span, CancellationToken token)
        {
            PrepareTask();

            SmartFarmerLog.Debug($"providing water for {span.TotalSeconds} seconds");
            await Task.Delay(span);

            EndTask();
            await Task.CompletedTask;
        }
    }
}