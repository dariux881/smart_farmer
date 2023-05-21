
using System;
using Quartz;

namespace SmartFarmer.OperationalManagement.Jobs;

public class RunPlanRequestEventArgs : EventArgs
{
    public IJobExecutionContext Context { get; }

    public RunPlanRequestEventArgs(IJobExecutionContext context)
    {
        Context = context;
    }
}
