using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public class FarmerMoveArmAtHeight : FarmerBaseTask, IFarmerMoveArmAtHeight
{
    private int _currentHeight;
    private IFarmerDeviceHandler _deviceHandler;

    public FarmerMoveArmAtHeight(IFarmerDeviceHandler handler)
    {
        RequiredTool = FarmerTool.None;
        _deviceHandler = handler;
    }

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        if (parameters == null || parameters.Length < 1) throw new ArgumentException(nameof(parameters));

        var height = (int)parameters[0];

        await MoveToHeight(height, token);
    }

    public async Task MoveToHeight(int heightInCm, CancellationToken token)
    {
        PrepareTask();

        SmartFarmerLog.Debug($"moving to height {heightInCm} cm");
        
        var result = await _deviceHandler.MoveArmAtheightAsync(heightInCm, token);
        if (!result)
        {
            var message = "Error in changing height";

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

        _currentHeight = heightInCm;

        SmartFarmerLog.Debug($"now on {heightInCm} cm");

        EndTask();
    }

    public int GetCurrentHeight()
    {
        return _currentHeight;
    }
}