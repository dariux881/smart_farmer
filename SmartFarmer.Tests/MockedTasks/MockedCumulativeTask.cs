using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Health;
using SmartFarmer.Utils;

namespace SmartFarmer.MockedTasks.GenericCollection;

public class MockedCumulativeTask : IFarmerLeavesStatusCheckerTask, IFarmerParasiteCheckerTask
{
    public MockedCumulativeTask() 
    {
        ID = Extensions.RandomString(15);
    }

    public FarmerTool RequiredTool => FarmerTool.None;

    public string TaskName => this.GetType().FullName;

    public string ID { get; private set; }

    public bool IsInProgress { get; set; }

    public Exception LastException { get; set; }

    public async Task Execute(CancellationToken token)
    {
        IsInProgress = true;
        LastException = null;

        IsInProgress = false;

        await Task.CompletedTask;
    }
}