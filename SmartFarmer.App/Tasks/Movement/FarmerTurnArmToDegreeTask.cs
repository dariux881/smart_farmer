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

public class FarmerTurnArmToDegreeTask : FarmerBaseTask, IFarmerTurnArmToDegreeTask
{
    private double _currentDegrees;
    private IFarmerTurnToolDevice _deviceHandler;

    public FarmerTurnArmToDegreeTask(IFarmerTurnToolDevice handler)
    {
        RequiredTool = FarmerTool.None;
        _deviceHandler = handler;
    }

    public override string TaskName => "Turn arm task";

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        if (parameters == null || parameters.Length < 1) throw new ArgumentException(nameof(parameters));

        var degrees = (double)parameters[0];

        await TurnArmToDegrees(degrees, token);
    }

    public async Task TurnArmToDegrees(double degrees, CancellationToken token)
    {
        PrepareTask();

        SmartFarmerLog.Debug($"turning at {degrees} degrees");

        var result = await _deviceHandler.TurnArmToDegreesAsync(degrees, token);
        if (!result)
        {
            var message = "Error in turning arm";

            EndTask(true);

            throw new FarmerTaskExecutionException(
                this.ID,
                null,
                message,
                null, AlertCode.BlockedTurningArm, AlertLevel.Error, AlertSeverity.High);
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