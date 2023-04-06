using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement
{
    public class FarmerMoveArmAtHeight : FarmerBaseTask, IFarmerMoveArmAtHeight
    {
        private double _currentHeight;
        
        public FarmerMoveArmAtHeight()
        {
            RequiredTool = FarmerTool.None;
        }

        public override async Task Execute(object[] parameters, CancellationToken token)
        {
            if (parameters == null || parameters.Length < 1) throw new ArgumentException(nameof(parameters));

            var height = (int)parameters[0];

            await MoveToHeight(height, token);
        }

        public async Task MoveToHeight(double heightInCm, CancellationToken token)
        {
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