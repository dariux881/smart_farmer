using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Health;
using SmartFarmer.Utils;

public class MockedLeavesStatusChecker : IFarmerLeavesStatusChecker, IFarmerParasiteChecker
{
    public MockedLeavesStatusChecker() 
    {
        ID = StringUtils.RandomString(15);
    }

    public FarmerTool RequiredTool => FarmerTool.None;

    public string ID { get; private set; }

    public bool IsInProgress { get; set; }

    public Exception? LastException { get; set; }

    public async Task Execute(object[]? parameters, CancellationToken token)
    {
        IsInProgress = true;
        LastException = null;

        IsInProgress = false;

        await Task.CompletedTask;
    }
}