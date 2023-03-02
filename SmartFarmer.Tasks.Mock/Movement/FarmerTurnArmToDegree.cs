using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement
{
    public class FarmerTurnArmToDegree : FarmerBaseTask, IFarmerTurnArmToDegree
    {
        private double _currentDegrees;
        
        public FarmerTurnArmToDegree()
        {
            RequiredTool = FarmerTool.None;
        }

        public override async Task Execute(object[] parameters, CancellationToken token)
        {
            if (parameters == null || parameters.Length < 1) throw new ArgumentException(nameof(parameters));

            var degrees = (double)parameters[0];

            await TurnArmToDegree(degrees, token);
        }

        public async Task TurnArmToDegree(double degrees, CancellationToken token)
        {
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