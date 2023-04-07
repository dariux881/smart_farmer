using System;
using System.Threading.Tasks;
using Quartz;
using SmartFarmer.Misc;

namespace SmartFarmer.OperationalManagement.Jobs;

public class CheckPlanJob : IFarmerSchedulerJob
{    
    public async Task Execute(IJobExecutionContext context)
    {
        SmartFarmerLog.Debug("checking plans to run");
        context.Result = new SchedulerJobEventArgs(AutoAppOperation.CheckPlansToRun, null);

        await Task.CompletedTask;
    }
}