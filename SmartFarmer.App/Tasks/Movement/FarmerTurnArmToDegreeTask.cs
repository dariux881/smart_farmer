using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Exceptions;
using SmartFarmer.FarmerLogs;
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
    public double TargetDegrees { get; set; }

    public override void ConfigureTask(IDictionary<string, string> parameters)
    {
        var key = nameof(TargetDegrees);
        
        if (parameters != null && parameters.ContainsKey(key))
        {
            TargetDegrees = double.Parse(parameters[key], System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public override async Task<object> Execute(CancellationToken token)
    {
        return await TurnArmToDegrees(TargetDegrees, token);
    }

    public async Task<object> TurnArmToDegrees(double degrees, CancellationToken token)
    {
        TargetDegrees = degrees;
        
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

        return null;
    }

    public double GetCurrentDegrees()
    {
        return _currentDegrees;
    }
}