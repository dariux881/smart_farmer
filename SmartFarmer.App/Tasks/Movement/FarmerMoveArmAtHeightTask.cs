using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Exceptions;
using SmartFarmer.FarmerLogs;
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
    public double TargetHeightInCm { get; set; }

    public async Task<bool> InitializeAsync(CancellationToken token)
    {
        // physical initialization must be performed by the external device
        await Task.CompletedTask;
        return true;
    }

    public override async Task<object> Execute(CancellationToken token)
    {
        return await MoveToHeight(TargetHeightInCm, token);
    }

    public async Task<object> MoveToHeight(double heightInCm, CancellationToken token)
    {
        TargetHeightInCm = heightInCm;
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

        return null;
    }

    public double GetCurrentHeight()
    {
        return _currentHeight;
    }
}