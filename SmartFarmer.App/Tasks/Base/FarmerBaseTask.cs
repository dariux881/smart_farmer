using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Base;

public abstract class FarmerBaseTask : IFarmerTask
{
    public string ID { get; set; }
    public FarmerTool RequiredTool { get; protected set; }
    public bool IsInProgress { get; protected set; }

    public Exception LastException { get; protected set; }
    public abstract string TaskName { get; }

    public abstract Task Execute(CancellationToken token);

    protected void PrepareTask()
    {
        IsInProgress = true;
        SmartFarmerLog.Information($"starting task \"{this.GetTaskName()}\"");
    }

    protected void EndTask(bool error = false)
    {
        var withError = error ? " with error" : "";

        if (error)
        {
            SmartFarmerLog.Error($" task \"{this.GetTaskName()}\" completed{withError}");
        }
        else
        {
            SmartFarmerLog.Information($" task \"{this.GetTaskName()}\" completed{withError}");
        }

        IsInProgress = false;
    }

}