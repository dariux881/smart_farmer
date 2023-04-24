using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Base;

namespace SmartFarmer.Tasks.Weed;

public class FarmerWeedRemovalTask : FarmerBaseTask, IFarmerWeedRemovalTask
{
    public FarmerWeedRemovalTask()
    {
        this.RequiredTool = Utils.FarmerTool.Weed;
    }

    public override string TaskName => "Removal of week task";

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        await DetectWeed();
        await RemoveWeed();
    }

    private async Task DetectWeed() {
        await Task.CompletedTask;
    }

    private async Task RemoveWeed() {
        await Task.CompletedTask;
    }
}

