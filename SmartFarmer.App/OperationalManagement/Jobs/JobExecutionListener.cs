
using System;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using SmartFarmer.FarmerLogs;

namespace SmartFarmer.OperationalManagement.Jobs;

public class JobExecutionListener : IJobListener
{
    public string Name => "Job Execution Listener";
    public event EventHandler CheckPlansToRunRequested;
    public event EventHandler<RunPlanRequestEventArgs> NewRunPlansRequested;

    public async Task JobExecutionVetoed(
        IJobExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    public async Task JobToBeExecuted(
        IJobExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    public async Task JobWasExecuted(
        IJobExecutionContext context, 
        JobExecutionException jobException, 
        CancellationToken cancellationToken = default)
    {
        // check exceptions
        if (jobException != null)
        {
            SmartFarmerLog.Exception(jobException);
        }

        // check result
        var result = context.Result as SchedulerJobEventArgs;
        if (result != null)
        {
            switch(result.Operation)
            {
                case AutoAppOperation.CheckPlansToRun:
                    CheckPlansToRunRequested?.Invoke(this, EventArgs.Empty);
                    break;

                case AutoAppOperation.RunPlan:
                    NewRunPlansRequested?.Invoke(this, new RunPlanRequestEventArgs(context));
                    break;
            }
        }

        await Task.CompletedTask;
    }
}