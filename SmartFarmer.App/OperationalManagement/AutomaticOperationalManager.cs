using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using SmartFarmer.Configurations;
using SmartFarmer.Data;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.OperationalManagement.Jobs;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.OperationalManagement;

public class AutomaticOperationalManager : OperationalModeManagerBase, IAutoOperationalModeManager
{
    private IScheduler _jobScheduler;
    private ConcurrentDictionary<string, List<string>> _scheduledPlansByGround;
    private const string CHECK_PLAN_GROUP = "checkPlanGroup";
    private const string SCHEDULED_PLAN_GROUP = "scheduledPlanGroup";

    public AutomaticOperationalManager(AppConfiguration appConfiguration)
    {
        _scheduledPlansByGround = new ConcurrentDictionary<string, List<string>>();
        PlanCheckSchedule = appConfiguration?.PlanCheckCronSchedule ?? "0 0/30 * ? * * *";
    }

    public override AppOperationalMode Mode => AppOperationalMode.Auto;

    public string PlanCheckSchedule { get; set; }

    public override string Name => "Automatic Operational Manager";

    public override void Dispose()
    {
        if (_jobScheduler != null && _jobScheduler.IsStarted)
        {
            _jobScheduler.Shutdown();
            _jobScheduler.Clear();
        }
    }

    public override async Task InitializeAsync()
    {
        await PrepareScheduler();
    }

    public override async Task Run(CancellationToken token)
    {
        // add scheduled plans
        await AddScheduledPlansToScheduler();

        // add other plans (to run just once)
        RunOneShotPlans();

        // start scheduler for automatic activities
        await StartScheduler(token);

        //TODO subscribe to Hub. New requests are added to the scheduler
    }

    public override void ProcessResult(OperationRequestEventArgs args)
    {
        if (args.ExecutionException != null)
        {
            SmartFarmerLog.Exception(args.ExecutionException);
        }

        if (args.Result != null)
        {
            SmartFarmerLog.Debug(args.Result);
        }
    }

    private void RunOneShotPlans()
    {
        var plans = GetPlansToRun();
        foreach (var plan in plans)
        {
            SendNewOperation(AppOperation.RunPlan, new[] { plan });
        }
    }

    private IEnumerable<string> GetPlansToRun()
    {
        var plans = new List<string>();

        foreach (var gGround in LocalConfiguration.Grounds.Values)
        {
            var ground = gGround as FarmerGround;
            if (ground == null) continue;
            
            var now = DateTime.UtcNow;
            var today = now.DayOfWeek;

            var plansInGround = 
                ground
                    .Plans
                        .Where(x => 
                            (x.ValidFromDt == null || x.ValidFromDt <= now) && // valid start
                            (x.ValidToDt == null || x.ValidToDt > now)) // valid end
                        .Where(x => string.IsNullOrEmpty(x.CronSchedule)) // not scheduled plan
                        .OrderBy(x => x.Priority)
                        .Select(x => x.ID)
                        .ToList();
            
            plans.AddRange(plansInGround);
        }

        return plans;
    }

    private async Task SendRunPlan(IJobExecutionContext context)
    {
        JobDataMap dataMap = context.MergedJobDataMap; 

        string planId = dataMap.GetString("planId");

        SendNewOperation(AppOperation.RunPlan, new [] { planId });
        await Task.CompletedTask;
    }

    private class JobExecutionListener : IJobListener
    {
        public string Name => "Job Execution Listener";
        private AutomaticOperationalManager _manager;

        public JobExecutionListener(AutomaticOperationalManager manager)
        {
            _manager = manager;
        }

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
                        _manager.RunOneShotPlans();
                        break;

                    case AutoAppOperation.RunPlan:
                        await _manager.SendRunPlan(context);
                        break;
                }
            }

            await Task.CompletedTask;
        }
    }

    private async Task PrepareScheduler()
    {
        StdSchedulerFactory factory = new StdSchedulerFactory();
        _jobScheduler = await factory.GetScheduler();

        IJobDetail job;
        ITrigger trigger;
        CreatePlanCheckJob(out job, out trigger);

        var listener = new JobExecutionListener(this);

        _jobScheduler
            .ListenerManager
            .AddJobListener(
                listener, 
                GroupMatcher<JobKey>.GroupEquals(SCHEDULED_PLAN_GROUP));

        // Tell Quartz to schedule the job using our trigger
        await _jobScheduler.ScheduleJob(job, trigger);
    }

    private async Task StartScheduler(CancellationToken token)
    {
        await _jobScheduler.Start(token);
    }

    private async Task AddScheduledPlansToScheduler()
    {
        foreach (var gGround in LocalConfiguration.Grounds.Values)
        {
            var ground = gGround as FarmerGround;
            if (ground == null) continue;
            
            var now = DateTime.UtcNow;
            var today = now.DayOfWeek;

            var plansInGround = 
                ground
                    .Plans
                        .Where(x => 
                            (x.ValidFromDt == null || x.ValidFromDt <= now) && // valid start
                            (x.ValidToDt == null || x.ValidToDt > now)) // valid end
                        .Where(x => !string.IsNullOrEmpty(x.CronSchedule)) // scheduled plan
                        //TODO .Where(x => IsValidCronSchedule(x.CronSchedule))
                        .OrderBy(x => x.Priority)
                        .ToList();
            
            foreach (var plan in plansInGround)
            {
                CreateScheduledPlanJob(plan, out var job, out var trigger);

                // Tell Quartz to schedule the job using our trigger
                await _jobScheduler.ScheduleJob(job, trigger);
            }

            _scheduledPlansByGround.TryAdd(ground.ID, plansInGround.Select(x => x.ID).ToList());
        }

    }

    private void CreatePlanCheckJob(out IJobDetail job, out ITrigger trigger)
    {
        job = JobBuilder.Create<CheckPlanJob>()
            .WithIdentity(name: "checkPlanJob", group: CHECK_PLAN_GROUP)
            .Build();

        trigger = TriggerBuilder.Create()
            .WithIdentity(name: "checkPlanTrigger", group: CHECK_PLAN_GROUP)
            .WithCronSchedule(PlanCheckSchedule)
            .Build();
    }

    private void CreateScheduledPlanJob(IFarmerPlan plan, out IJobDetail job, out ITrigger trigger)
    {
        job = JobBuilder.Create<ScheduledPlanJob>()
            .WithIdentity(name: "scheduledPlan_" + plan.ID, group: SCHEDULED_PLAN_GROUP)
            .UsingJobData("planId", plan.ID)
            .Build();

        trigger = TriggerBuilder.Create()
            .WithIdentity(name: "scheduledPlanTrigger_" + plan.ID, group: SCHEDULED_PLAN_GROUP)
            .WithCronSchedule(plan.CronSchedule)
            .Build();
    }
}