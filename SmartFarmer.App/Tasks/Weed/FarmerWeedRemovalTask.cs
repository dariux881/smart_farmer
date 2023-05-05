using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Exceptions;
using SmartFarmer.Movement;
using SmartFarmer.Tasks.Base;

namespace SmartFarmer.Tasks.Weed;

public class FarmerWeedRemovalTask : FarmerBaseTask, IFarmerWeedRemovalTask
{
    private IFarmerDevicePositionManager _device;
    private Queue<Farmer2dPoint> _weedPositionQueue;
    
    public FarmerWeedRemovalTask(IFarmerDevicePositionManager device)
    {
        this.RequiredTool = Utils.FarmerTool.Weed;
        _weedPositionQueue = new Queue<Farmer2dPoint>();

        _device = device;
    }

    public override string TaskName => "Removal of week task";

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        Exception _ex = null;
        PrepareTask();

        try
        {
            await DetectWeed(token);
            await RemoveWeed(token);
        }
        catch(Exception ex)
        {
            _ex = ex;
            throw;
        }
        finally
        {
            EndTask(_ex != null);
        }
    }

    private async Task DetectWeed(CancellationToken token) {
        //TODO enqueue in _weedPositionQueue
        await Task.CompletedTask;
    }

    private async Task RemoveWeed(CancellationToken token) 
    {
        try 
        {
            while (_weedPositionQueue.Count > 0)
            {
                // restore height
                await _device.MoveArmAtMaxHeightAsync(token);

                // go to weed position
                var pos = _weedPositionQueue.Dequeue();
                await _device.MoveOnGridAsync(pos.X, pos.Y, token);

                // destroy it
                await _device.MoveArmAtHeightAsync(0, token);
            }
        }
        catch(FarmerTaskExecutionException tex)
        {
            throw new FarmerTaskExecutionException(
                this.ID,
                tex.PlantId,
                tex.Message,
                tex,
                tex.Code,
                tex.Level,
                tex.Severity
            );
        }
        catch(Exception ex)
        {
            throw new FarmerTaskExecutionException(
                this.ID,
                null,
                "failing removing weed",
                ex,
                Alerts.AlertCode.Unknown,
                Alerts.AlertLevel.Error,
                Alerts.AlertSeverity.High
            );
        }

        await Task.CompletedTask;
    }
}

