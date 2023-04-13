using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Exceptions;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public class FarmerMoveArmAtMaxHeightTask : FarmerBaseTask, IFarmerMoveArmAtMaxHeightTask
{
    private double _currentHeight = double.NaN;
    private IFarmerMoveAtHeightDevice _deviceHandler;

    public FarmerMoveArmAtMaxHeightTask(IFarmerMoveAtHeightDevice handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        RequiredTool = FarmerTool.None;
        _deviceHandler = handler;
    }

    public async Task<bool> Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
        return true;
    }

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        await MoveToMaxHeight(token);
    }

    public async Task MoveToMaxHeight(CancellationToken token)
    {
        PrepareTask();

        SmartFarmerLog.Debug("moving to max height");
        
        var height = await _deviceHandler.MoveArmAtMaxHeightAsync(token);
        if (height > 0)
        {
            var message = "Error in moving to max height";

            EndTask(true);
            
            throw new FarmerTaskExecutionException(
                this.ID,
                null,
                message,
                null, AlertCode.BlockedArm, AlertLevel.Error, AlertSeverity.High);
        }

        _currentHeight = height;

        SmartFarmerLog.Debug($"now on {height} cm");

        EndTask();
    }

    public double GetCurrentHeight()
    {
        return _currentHeight;
    }
}