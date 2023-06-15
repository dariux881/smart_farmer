using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement
{
    public class FarmerMoveArmAtHeight : FarmerBaseTask, IFarmerMoveArmAtHeightTask
    {
        private double _currentHeight;
        
        public FarmerMoveArmAtHeight()
        {
            RequiredTool = FarmerTool.None;
        }

        public double TargetHeightInCm { get; set; }

        public override async Task Execute(CancellationToken token)
        {
            await MoveToHeight(TargetHeightInCm, token);
        }

        public async Task MoveToHeight(double heightInCm, CancellationToken token)
        {
            TargetHeightInCm = heightInCm;

            PrepareTask();

            SmartFarmerLog.Debug($"moving to height {heightInCm} cm");
            await Task.Delay(1000);
            _currentHeight = heightInCm;
            SmartFarmerLog.Debug($"now on {heightInCm} cm");

            EndTask();

            await Task.CompletedTask;
        }
          
        public double GetCurrentHeight()
        {
            return _currentHeight;
        }
    }
}