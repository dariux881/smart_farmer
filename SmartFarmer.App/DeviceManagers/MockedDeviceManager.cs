using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;

namespace SmartFarmer.DeviceManagers;

public class MockedDeviceManager : IFarmerDeviceManager
{
    public double Alpha { get; private set; }

    public double Beta { get; private set; }

    public double Z { get; private set; }

    public double X { get; private set; }

    public double Y { get; private set; }

    public event EventHandler NewPoint;

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

        Z = heightInCm;
        NewPoint?.Invoke(this, EventArgs.Empty);

        await Task.CompletedTask;
        return true;
    }

    public async Task<double> MoveArmAtMaxHeightAsync(CancellationToken token)
    {
        Z = 100;
        NewPoint?.Invoke(this, EventArgs.Empty);

        await Task.CompletedTask;
        return Z;
    }

    public async Task<bool> MoveOnGridAsync(double x, double y, CancellationToken token)
    {
        if (x.IsNan() || y.IsNan())
        {
            SmartFarmerLog.Warning($"no valid grid position: {x}/{y}");
            return false;
        }

        X = x;
        Y = y;
        NewPoint?.Invoke(this, EventArgs.Empty);
        
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> MoveToPosition(IFarmer5dPoint position, CancellationToken token)
    {
        bool moveResult = false;

        moveResult = await MoveOnGridAsync(position.X, position.Y, token);
        moveResult |= await MoveArmAtHeightAsync(position.Z, token);
        moveResult |= await TurnArmToDegreesAsync(position.Alpha, token);
        moveResult |= await PointDeviceAsync(position.Beta, token);

        await Task.Delay(10000);

        return moveResult;
    }

    public async Task<bool> PointDeviceAsync(double degrees, CancellationToken token)
    {
        if (degrees.IsNan())
        {
            SmartFarmerLog.Warning("no valid pointing device degree");
            return false;
        }

        Beta = degrees;
        NewPoint?.Invoke(this, EventArgs.Empty);
        
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

        Alpha = degrees;
        NewPoint?.Invoke(this, EventArgs.Empty);
        
        await Task.CompletedTask;
        return true;
    }
}