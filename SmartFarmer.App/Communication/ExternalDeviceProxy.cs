
using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Tasks.Movement;

/// <summary>
/// Implements a proxy pattern towards an external device (e.g. Arduino)
/// </summary>
public class ExternalDeviceProxy : 
    IFarmerDeviceManager,
    IDisposable
{
    private Farmer5dPositionNotifier _positionNotifier;

    public ExternalDeviceProxy()
    {
        _positionNotifier = new Farmer5dPositionNotifier();
        _positionNotifier.NewPoint += NewPointReceived;
    }

    public double X => _positionNotifier.X;
    public double Y => _positionNotifier.Y;
    public double Z => _positionNotifier.Z;
    public double Alpha => _positionNotifier.Alpha;
    public double Beta => _positionNotifier.Beta;

    public event EventHandler NewPoint;

    public async Task<bool> MoveArmAtHeightAsync(double heightInCm, CancellationToken token)
    {
        await Task.CompletedTask;
        //TODO implement

        _positionNotifier.Z = heightInCm;

        return true;
    }

    public async Task<bool> MoveArmAtMaxHeightAsync(CancellationToken token)
    {
        await Task.CompletedTask;
        //TODO implement. 

        _positionNotifier.Z = 100.0;

        return true;
    }

    public async Task<bool> MoveOnGridAsync(double x, double y, CancellationToken token)
    {
        await Task.CompletedTask;
        
        //TODO implement
        _positionNotifier.X = x;
        _positionNotifier.Y = y;

        return true;
    }

    public async Task<bool> PointDeviceAsync(double degrees, CancellationToken token)
    {
        await Task.CompletedTask;
        //TODO implement

        _positionNotifier.Beta = degrees;

        return true;
    }

    public async Task<bool> TurnArmToDegreesAsync(double degrees, CancellationToken token)
    {
        await Task.CompletedTask;
        //TODO implement

        _positionNotifier.Alpha = degrees;

        return true;
    }

    public void Dispose()
    {
        _positionNotifier.NewPoint -= NewPointReceived;
    }

    private void NewPointReceived(object sender, EventArgs args)
    {
        if (sender is IFarmerPointNotifier notifier)
        {
            NewPoint.Invoke(this, args);
        }
    }
}
