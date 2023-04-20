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
        DetectWeed();
        RemoveWeed();

        await Task.CompletedTask;
    }

    private void DetectWeed() {

    }

    private void RemoveWeed() {

    }
}

