using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement
{
    public class FarmerTurnArmToDegree : FarmerBaseTask, IFarmerTurnArmToDegreeTask
    {
        private double _currentDegrees;
        
        public FarmerTurnArmToDegree()
        {
            RequiredTool = FarmerTool.None;
        }

        public double TargetDegrees { get; set; }

        public override async Task Execute(CancellationToken token)
        {
            await TurnArmToDegrees(TargetDegrees, token);
        }

        public async Task TurnArmToDegrees(double degrees, CancellationToken token)
        {
            TargetDegrees = degrees;
            
            PrepareTask();

            SmartFarmerLog.Debug($"turning at {degrees} degrees");
            await Task.Delay(1000);
            _currentDegrees = degrees;
            SmartFarmerLog.Debug($"now at {degrees} degrees");

            EndTask();

            await Task.CompletedTask;
        }
        
        public double GetCurrentDegrees()
        {
            return _currentDegrees;
        }
    }
}