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
            if (parameters == null || parameters.Length < 2) throw new ArgumentException(nameof(parameters));

            var pump = parameters[0].GetInt();
            var amount = parameters[1].GetDouble();
 
            await ProvideWater(pump, amount, token);
        }

        public async Task ProvideWater(int pumpNumber, double amountInLiters, CancellationToken token)
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