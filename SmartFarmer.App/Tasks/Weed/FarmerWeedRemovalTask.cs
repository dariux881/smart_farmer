using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        await DetectWeed(token);
        await RemoveWeed(token);
    }

    private async Task DetectWeed(CancellationToken token) {
        //TODO enqueue in _weedPositionQueue
        await Task.CompletedTask;
    }

    private async Task RemoveWeed(CancellationToken token) {
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

        await Task.CompletedTask;
    }
}

