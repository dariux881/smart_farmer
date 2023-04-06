using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public class FarmerMoveArmAtHeight : FarmerBaseTask, IFarmerMoveArmAtHeight
{
    private double _currentHeight = double.NaN;
    private IFarmerMoveAtHeightDevice _deviceHandler;

    public FarmerMoveArmAtHeight(IFarmerMoveAtHeightDevice handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        RequiredTool = FarmerTool.None;
        _deviceHandler = handler;
    }

    public async Task<bool> Initialize(CancellationToken token)
    {
        var desiredHeight = 0.0;
        var result = await _deviceHandler.MoveArmAtMaxHeightAsync(token);

        if (result) _currentHeight = desiredHeight;

        return result;
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

            await SmartFarmerLog
                .Error(
                    message, 
                    new FarmerAlertRequestData()
                    {
                        Message = message,
                        RaisedByTaskId = this.ID,
                        Level = AlertLevel.Error,
                        Severity = AlertSeverity.High,
                        Code = AlertCode.BlockedArm
                    });

            EndTask();
            return;
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