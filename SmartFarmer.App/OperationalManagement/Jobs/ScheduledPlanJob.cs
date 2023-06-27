using System.Threading.Tasks;
using Quartz;
using SmartFarmer.FarmerLogs;

namespace SmartFarmer.OperationalManagement.Jobs;

public class ScheduledPlanJob : IFarmerSchedulerJob
{
    
    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap dataMap = context.MergedJobDataMap; 

        string planId = dataMap.GetString("planId");    
        SmartFarmerLog.Debug("running plan " + planId);

        context.Result = new SchedulerJobEventArgs(AutoAppOperation.RunPlan, new [] {planId});

        await Task.CompletedTask;
    }
}