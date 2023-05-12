using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Movement;

namespace SmartFarmer.Communication;

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
        Z = heightInCm;

        await Task.CompletedTask;
        return true;
    }

    public async Task<double> MoveArmAtMaxHeightAsync(CancellationToken token)
    {
        Z = 100;

        await Task.CompletedTask;
        return Z;
    }

    public async Task<bool> MoveOnGridAsync(double x, double y, CancellationToken token)
    {
        X = x;
        Y = y;
        
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> MoveToPosition(IFarmer5dPoint position, CancellationToken token)
    {
        X = position.X;
        Y = position.Y;
        Z = position.Z;
        Alpha = position.Alpha;
        Beta = position.Beta;
        
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> PointDeviceAsync(double degrees, CancellationToken token)
    {
        Beta = degrees;
        
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> TurnArmToDegreesAsync(double degrees, CancellationToken token)
    {
        Alpha = degrees;
        
        await Task.CompletedTask;
        return true;
    }
}