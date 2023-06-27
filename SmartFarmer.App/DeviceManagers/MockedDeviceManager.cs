using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Position;

namespace SmartFarmer.DeviceManagers;

public class MockedDeviceManager : IFarmerDeviceManager
{
    public MockedDeviceManager()
    {
        DevicePosition = new Farmer5dPoint();

        DevicePosition.NewPoint += NewPointReceived;
    }

    public event EventHandler NewPoint;
    public Farmer5dPoint DevicePosition { get; }

    public async Task<double> GetCurrentHumidityLevel(CancellationToken token)
    {
        await Task.CompletedTask;
        return 5;
    }

    public async Task<bool> ProvideWaterAsync(int pumpNumber, double amountInLiters, CancellationToken token)
    {
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> MoveArmAtHeightAsync(double heightInCm, CancellationToken token)
    {
        if (heightInCm.IsNan())
        {
            SmartFarmerLog.Warning("no valid height");
            return false;
        }

        DevicePosition.Z = heightInCm;

        await Task.CompletedTask;
        return true;
    }

    public async Task<double> MoveArmAtMaxHeightAsync(CancellationToken token)
    {
        DevicePosition.Z = 100;

        await Task.CompletedTask;
        return DevicePosition.Z;
    }

    public async Task<bool> MoveOnGridAsync(double x, double y, CancellationToken token)
    {
        if (x.IsNan() || y.IsNan())
        {
            SmartFarmerLog.Warning($"no valid grid position: {x}/{y}");
            return false;
        }

        DevicePosition.X = x;
        DevicePosition.Y = y;
        
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> MoveToPosition(Farmer5dPoint position, CancellationToken token)
    {
        bool moveResult = false;

        moveResult = await MoveOnGridAsync(position.X, position.Y, token);
        moveResult |= await MoveArmAtHeightAsync(position.Z, token);
        moveResult |= await TurnArmToDegreesAsync(position.Alpha, token);
        moveResult |= await PointDeviceAsync(position.Beta, token);

        await Task.Delay(10000, token);

        return moveResult;
    }

    public async Task<bool> PointDeviceAsync(double degrees, CancellationToken token)
    {
        if (degrees.IsNan())
        {
            SmartFarmerLog.Warning("no valid pointing device degree");
            return false;
        }

        DevicePosition.Beta = degrees;
        
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> TurnArmToDegreesAsync(double degrees, CancellationToken token)
    {
        if (degrees.IsNan())
        {
            SmartFarmerLog.Warning("no valid turning angle");
            return false;
        }

        DevicePosition.Alpha = degrees;
        
        await Task.CompletedTask;
        return true;
    }

    private void NewPointReceived(object sender, EventArgs args)
    {
        NewPoint?.Invoke(this, args);
    }

}