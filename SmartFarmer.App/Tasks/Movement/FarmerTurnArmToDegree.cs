using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public class FarmerTurnArmToDegree : FarmerBaseTask, IFarmerTurnArmToDegree
{
    private double _currentDegrees;
    private IFarmerDeviceHandler _deviceHandler;

    public FarmerTurnArmToDegree(IFarmerDeviceHandler handler)
    {
        RequiredTool = FarmerTool.None;
        _deviceHandler = handler;
    }

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

            SmartFarmerLog
                .Error(
                    message, 
                    new FarmerAlert()
                    {
                        When = DateTime.UtcNow,
                        Message = message,
                        RaisedByTaskId = this.ID,
                        Level = AlertLevel.Error,
                        Severity = AlertSeverity.High,
                        Code = AlertCode.BlockedArm
                    });
                    
            EndTask();
            return;
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