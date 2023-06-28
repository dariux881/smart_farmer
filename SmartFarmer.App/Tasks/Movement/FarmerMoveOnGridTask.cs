using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Exceptions;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Movement;
using SmartFarmer.Position;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public class FarmerMoveOnGridTask : FarmerBaseTask, IFarmerMoveOnGridTask, IRequiresInitialization, IDisposable
{
    private Farmer2dPoint _currentPosition;
    private IFarmerMoveOnGridDevice _deviceHandler;


    public FarmerMoveOnGridTask(IFarmerGarden garden, IFarmerMoveOnGridDevice handler)
    {
        if (garden == null) throw new ArgumentNullException(nameof(garden));
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        RequiredTool = FarmerTool.None;
        _deviceHandler = handler;

        InitCurrentPosition(garden);
    }

    public override string TaskName => "Move on Grid task";
    public double TargetXInCm { get; set; }
    public double TargetYInCm { get; set; }

    public override void ConfigureTask(IDictionary<string, string> parameters)
    {
        var key = nameof(TargetXInCm);
        
        if (parameters != null && parameters.ContainsKey(key))
        {
            TargetXInCm = double.Parse(parameters[key], System.Globalization.CultureInfo.InvariantCulture);
        }

        key = nameof(TargetYInCm);
        
        if (parameters != null && parameters.ContainsKey(key))
        {
            TargetYInCm = double.Parse(parameters[key], System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public async Task<bool> InitializeAsync(CancellationToken token)
    {
        // physical initialization must be performed by the external device
        await Task.CompletedTask;
        return true;
    }

    public override async Task<object> Execute(CancellationToken token)
    {
        return await MoveToPosition(TargetXInCm, TargetYInCm, token);
    }

    public async Task<object> MoveToPosition(double x, double y, CancellationToken token)
    {
        TargetXInCm = x;
        TargetYInCm = y;
        
        PrepareTask();

        SmartFarmerLog.Debug($"moving to {x}, {y}");

        var result = await _deviceHandler.MoveOnGridAsync(x, y, token);
        if (!result)
        {
            var message = "Error in moving device on grid";

            EndTask(true);

            throw new FarmerTaskExecutionException(
                this.ID,
                null,
                message,
                null, AlertCode.BlockedOnGrid, AlertLevel.Error, AlertSeverity.High);
        }

        SmartFarmerLog.Debug($"now on {x}, {y}");

        EndTask();

        return null;
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

    private void InitCurrentPosition(IFarmerGarden garden)
    {
        _currentPosition = 
            new Farmer2dPoint(
                0.0, 0.0, // expected 0,0 -> to reset when initializing
                garden?.WidthInMeters,
                garden?.LengthInMeters);
    }
}
