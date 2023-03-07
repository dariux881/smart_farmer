using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public class FarmerMoveOnGridTask : FarmerBaseTask, IFarmerMoveOnGridTask, IDisposable
{
    private FarmerPoint _currentPosition;
    private IFarmerDeviceHandler _deviceHandler;

    public FarmerMoveOnGridTask(IFarmerGround ground, IFarmerDeviceHandler handler)
    {
        RequiredTool = FarmerTool.None;
        _deviceHandler = handler;

        InitPosition(ground);
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
            SmartFarmerLog.Error("Error in moving device on grid", true);
            EndTask();
            return;
        }

        //TODO update position

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

    private void InitPosition(IFarmerGround ground)
    {
        _currentPosition = 
            new FarmerPoint(
                0.0, 0.0, // expected 0,0 -> to reset when initializing
                _deviceHandler,
                ground?.WidthInMeters,
                ground?.LengthInMeters);
    }
}
