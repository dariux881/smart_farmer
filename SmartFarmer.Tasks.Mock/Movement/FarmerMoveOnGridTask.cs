using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement
{
    public class FarmerMoveOnGridTask : FarmerBaseTask, IFarmerMoveOnGridTask
    {
        public FarmerMoveOnGridTask()
        {
            RequiredTool = FarmerTool.None;
        }

        public override async Task Execute(object[] parameters, CancellationToken token)
        {
            if (parameters == null || parameters.Length < 2) throw new ArgumentException(nameof(parameters));

            var x = (int)parameters[0];
            var y = (int)parameters[1];

            await MoveToPosition(x, y, token);
        }

        public async Task MoveToPosition(int x, int y, CancellationToken token)
        {
            PrepareTask();

            SmartFarmerLog.Debug($"moving to {x}, {y}");
            await Task.Delay(1000);
            SmartFarmerLog.Debug($"now on {x}, {y}");

            EndTask();

            await Task.CompletedTask;
        }
    }
}