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

public class FarmerPointTargetTask : FarmerBaseTask, IFarmerPointTargetTask
{
    private double _currentDegrees;
    private IFarmerTurnToolDevice _deviceHandler;

    public FarmerPointTargetTask(IFarmerTurnToolDevice handler)
    {
        RequiredTool = FarmerTool.None;
        _deviceHandler = handler;
    }

    public override string TaskName => "Point target task";
    public double TargetDegrees { get; set; }

    public override async Task<object> Execute(CancellationToken token)
    {
        return await TurnDeviceToDegrees(TargetDegrees, token);
    }

    public async Task<object> TurnDeviceToDegrees(double degrees, CancellationToken token)
    {
        TargetDegrees = degrees;
        PrepareTask();

        SmartFarmerLog.Debug($"pointing target at {degrees} degrees");

        var result = await _deviceHandler.PointDeviceAsync(degrees, token);
        if (!result)
        {
            var message = "Error in pointing target";

            EndTask(true);

            throw new FarmerTaskExecutionException(
                this.ID,
                null,
                message,
                null, AlertCode.BlockedPointingTarget, AlertLevel.Error, AlertSeverity.High);
        }
        
        _currentDegrees = degrees;
        SmartFarmerLog.Debug($"now at {degrees} degrees");

        EndTask();

        return null;
    }

    public double GetCurrentDegrees()
    {
        return _currentDegrees;
    }
}