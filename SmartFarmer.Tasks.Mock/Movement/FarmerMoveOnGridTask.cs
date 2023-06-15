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

        public double TargetXInCm { get; set; }
        public double TargetYInCm { get; set; }

        public override async Task Execute(CancellationToken token)
        {
            await MoveToPosition(TargetXInCm, TargetYInCm, token);
        }

        public async Task MoveToPosition(double x, double y, CancellationToken token)
        {
            TargetXInCm = x;
            TargetYInCm = y;

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