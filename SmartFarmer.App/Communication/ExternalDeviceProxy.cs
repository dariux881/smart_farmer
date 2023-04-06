
using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Movement;

public class ExternalDeviceProxy : 
    IFarmerMoveOnGridDevice,
    IFarmerMoveAtHeightDevice,
    IFarmerTurnToolDevice, 
    IDisposable
{
    private FarmerPositionNotifier _notifier;

    public ExternalDeviceProxy()
    {
        _notifier = new FarmerPositionNotifier();

        _notifier.NewPoint += NewPointReceived;
    }

    public double X => _notifier.X;

    public double Y => _notifier.Y;

    public event EventHandler NewPoint;

    public async Task<bool> MoveArmAtHeightAsync(double heightInCm, CancellationToken token)
    {
        await Task.CompletedTask;
        //TODO implement
        return true;
    }

    public async Task<bool> MoveArmAtMaxHeightAsync(CancellationToken token)
    {
        await Task.CompletedTask;
        //TODO implement
        return true;
    }

    public async Task<bool> MoveOnGridAsync(FarmerPoint position, CancellationToken token)
    {
        await Task.CompletedTask;
        
        _notifier.X = position.X;
        _notifier.Y = position.Y;

        return true;
    }

    public async Task<bool> PointDeviceAsync(double degrees, CancellationToken token)
    {
        await Task.CompletedTask;
        //TODO implement
        return true;
    }

    public async Task<bool> TurnArmToDegreesAsync(double degrees, CancellationToken token)
    {
        await Task.CompletedTask;
        //TODO implement
        return true;
    }

    public void Dispose()
    {
        _notifier.NewPoint -= NewPointReceived;
    }

    private void NewPointReceived(object sender, EventArgs args)
    {
        if (sender is IFarmerPointNotifier notifier)
        {
            NewPoint.Invoke(this, args);
        }
    }
}
