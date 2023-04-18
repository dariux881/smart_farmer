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

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        if (parameters == null || parameters.Length < 1) throw new ArgumentException(nameof(parameters));

        var degrees = (double)parameters[0];

        await TurnDeviceToDegrees(degrees, token);
    }

    public async Task TurnDeviceToDegrees(double degrees, CancellationToken token)
    {
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
    }

    public double GetCurrentDegrees()
    {
        return _currentDegrees;
    }
}