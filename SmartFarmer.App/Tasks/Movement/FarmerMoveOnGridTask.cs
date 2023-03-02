using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public class FarmerMoveOnGridTask : FarmerBaseTask, IFarmerMoveOnGridTask
{
    private FarmerPoint _currentPosition;

    public FarmerMoveOnGridTask(IFarmerGround ground)
    {
        RequiredTool = FarmerTool.None;

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
        throw new NotImplementedException();
        SmartFarmerLog.Debug($"now on {x}, {y}");

        EndTask();

        await Task.CompletedTask;
    }

    public void GetCurrentPosition(out double x, out double y)
    {
        x = _currentPosition.X;
        y = _currentPosition.Y;
    }

    private void InitPosition(IFarmerGround ground)
    {
        _currentPosition = 
            new FarmerPoint(
                0.0, 0.0, // expected 0,0 -> to reset when initializing
                new FarmerPositionNotifier(),
                ground?.WidthInMeters,
                ground?.LengthInMeters);
    }
}
