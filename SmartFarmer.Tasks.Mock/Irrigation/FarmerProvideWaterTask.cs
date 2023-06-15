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

        public int PumpNumber { get; set; }
        public double WaterAmountInLiters { get; set; }

        public override async Task Execute(CancellationToken token)
        {
            await ProvideWater(PumpNumber, WaterAmountInLiters, token);
        }

        public async Task ProvideWater(int pumpNumber, double amountInLiters, CancellationToken token)
        {
            PumpNumber = pumpNumber;
            WaterAmountInLiters = amountInLiters;
            
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