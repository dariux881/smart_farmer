using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement
{
    public class FarmerMoveOnGridTask : FarmerBaseTask, IFarmerMoveOnGridTask
    {
        private double _currentX, _currentY;

        public FarmerMoveOnGridTask()
        {
            RequiredTool = FarmerTool.None;
        }

        public override async Task Execute(object[] parameters, CancellationToken token)
        {
            if (parameters == null || parameters.Length < 2) throw new ArgumentException(nameof(parameters));

            var x = (double)parameters[0];
            var y = (double)parameters[1];

            await MoveToPosition(x, y, token);
        }

        public async Task MoveToPosition(double x, double y, CancellationToken token)
        {
            PrepareTask();

            SmartFarmerLog.Debug($"moving to {x}, {y}");
            await Task.Delay(1000);
            _currentX = x;
            _currentY = y;
            SmartFarmerLog.Debug($"now on {x}, {y}");

            EndTask();

            await Task.CompletedTask;
        }
            
        public void GetCurrentPosition(out double x, out double y)
        {
            x = _currentX;
            y = _currentY;
        }
    }
}