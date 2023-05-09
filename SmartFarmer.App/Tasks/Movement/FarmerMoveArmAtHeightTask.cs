using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Exceptions;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public class FarmerMoveArmAtHeightTask : FarmerBaseTask, IFarmerMoveArmAtHeightTask
{
    private double _currentHeight = double.NaN;
    private IFarmerMoveAtHeightDevice _deviceHandler;

    public FarmerMoveArmAtHeightTask(IFarmerMoveAtHeightDevice handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        RequiredTool = FarmerTool.None;
        _deviceHandler = handler;
    }

    public override string TaskName => "Move at Height task";

    public async Task<bool> Initialize(CancellationToken token)
    {
        // physical initialization must be performed by the external device
        await Task.CompletedTask;
        return true;
    }

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        if (parameters == null || parameters.Length < 1) throw new ArgumentException(nameof(parameters));

        var height = parameters[0].GetDouble();

        await MoveToHeight(height, token);
    }

    public async Task MoveToHeight(double heightInCm, CancellationToken token)
    {
        PrepareTask();

        SmartFarmerLog.Debug($"moving to height {heightInCm} cm");
        
        var result = await _deviceHandler.MoveArmAtHeightAsync(heightInCm, token);
        if (!result)
        {
            var message = "Error in changing height";

            EndTask(true);
            
            throw new FarmerTaskExecutionException(
                this.ID,
                null,
                message,
                null, AlertCode.BlockedArm, AlertLevel.Error, AlertSeverity.High);
        }

        _currentHeight = heightInCm;

        SmartFarmerLog.Debug($"now on {heightInCm} cm");

        EndTask();
    }

    public double GetCurrentHeight()
    {
        return _currentHeight;
    }
}