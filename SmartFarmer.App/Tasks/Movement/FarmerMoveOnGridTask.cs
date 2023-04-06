using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Data.Alerts;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public class FarmerMoveOnGridTask : FarmerBaseTask, IFarmerMoveOnGridTask, IDisposable
{
    private FarmerPoint _currentPosition;
    private IFarmerMoveOnGridDevice _deviceHandler;


    public FarmerMoveOnGridTask(IFarmerGround ground, IFarmerMoveOnGridDevice handler)
    {
        if (ground == null) throw new ArgumentNullException(nameof(ground));
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        RequiredTool = FarmerTool.None;
        _deviceHandler = handler;

        InitCurrentPosition(ground);
    }

    public async Task<bool> Initialize(CancellationToken token)
    {
        return await _deviceHandler.MoveOnGridAsync(new FarmerPoint(0, 0), token);
    }

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        if (parameters == null || parameters.Length < 2) throw new ArgumentException(nameof(parameters));

        double x, y;
        x = parameters[0].GetDouble();
        y = parameters[1].GetDouble();

        await MoveToPosition(x, y, token);
    }

    public async Task MoveToPosition(double x, double y, CancellationToken token)
    {
        PrepareTask();

        SmartFarmerLog.Debug($"moving to {x}, {y}");

        var result = await _deviceHandler.MoveOnGridAsync(new FarmerPoint(x, y), token);
        if (!result)
        {
            var message = "Error in moving device on grid";

            await SmartFarmerLog
                .Error(
                    message, 
                    new FarmerAlertRequestData()
                    {
                        Message = message,
                        RaisedByTaskId = this.ID,
                        Level = AlertLevel.Error,
                        Severity = AlertSeverity.High,
                        Code = AlertCode.BlockedOnGrid
                    });
                    
            EndTask();
            return;
        }

        SmartFarmerLog.Debug($"now on {x}, {y}");

        EndTask();
    }

    public void GetCurrentPosition(out double x, out double y)
    {
        x = _currentPosition.X;
        y = _currentPosition.Y;
    }

    public void Dispose()
    {
        _currentPosition?.Dispose();
    }

    private void InitCurrentPosition(IFarmerGround ground)
    {
        _currentPosition = 
            new FarmerPoint(
                0.0, 0.0, // expected 0,0 -> to reset when initializing
                _deviceHandler,
                ground?.WidthInMeters,
                ground?.LengthInMeters);
    }
}
