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
using SmartFarmer.Handlers;
using SmartFarmer.Misc;
using SmartFarmer.OperationalManagement.Jobs;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.OperationalManagement;

public class AutomaticOperationalManager : 
    OperationalModeManagerBase, 
    IAutoOperationalModeManager
{
    private IScheduler _jobScheduler;
    private ConcurrentDictionary<string, List<IJobDetail>> _scheduledPlanJobsByGround;
    private ConcurrentDictionary<string, ConcurrentQueue<string>> _plansToRunByGround;
    private SemaphoreSlim _groundProcessingSemaphore;
    private const string CHECK_PLAN_GROUP = "checkPlanGroup";
    private const string SCHEDULED_PLAN_GROUP = "scheduledPlanGroup";
    private readonly IFarmerLocalInformationManager _localInfoManager;
    private readonly IFarmerAppCommunicationHandler _communicationManager;
    private CancellationToken _operationsToken;

    public AutomaticOperationalManager(AppConfiguration appConfiguration)
    {
        _scheduledPlanJobsByGround = new ConcurrentDictionary<string, List<IJobDetail>>();
        _plansToRunByGround = new ConcurrentDictionary<string, ConcurrentQueue<string>>();

        _groundProcessingSemaphore = new SemaphoreSlim(1);
        PlanCheckSchedule = appConfiguration?.PlanCheckCronSchedule;

        _localInfoManager = FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true);
        _communicationManager = FarmerServiceLocator.GetService<IFarmerAppCommunicationHandler>(true);
    }

    public override AppOperationalMode Mode => AppOperationalMode.Auto;
    public string PlanCheckSchedule { get; }
    public override string Name => "Automatic Operational Manager";

    public override void Dispose()
    {
        if (_jobScheduler != null && _jobScheduler.IsStarted)
        {
            _jobScheduler.Shutdown();
            _jobScheduler.Clear();
        }

        if (_communicationManager != null)
        {
            _communicationManager.LocalGroundAdded -= LocalGroundAdded;
            _communicationManager.LocalGroundRemoved -= LocalGroundRemoved;
        }
    }

    public override async Task InitializeAsync(CancellationToken token)
    {
        await PrepareScheduler();
    }

    public override async Task Run(CancellationToken token)
    {
        _operationsToken = token;

        try
        {
            // add scheduled plans
            await AddScheduledPlansToScheduler();

            // add other plans (to run just once)
            RunOneShotPlans();

            // start scheduler for automatic activities
            await StartScheduler(token);

            // present grounds have been already processed. Subscribing to grounds change
            _communicationManager.LocalGroundAdded += LocalGroundAdded;
            _communicationManager.LocalGroundRemoved += LocalGroundRemoved;

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(5000);
            }

            await Task.CompletedTask;
        }
        catch(AggregateException ex)
        {
            SmartFarmerLog.Exception(ex);
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
        }

        SmartFarmerLog.Information("closing Auto manager");
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

        TryRunNextPlan(GroundUtils.GetGroundByPlan(args.AdditionalData.First())?.ID);
    }

    private void LocalGroundAdded(object sender, GroundChangedEventArgs e)
    {
        var groundId = e.GroundId;

        if (_scheduledPlanJobsByGround.ContainsKey(groundId))
        {
            // ground has been already processed. Recurrent jobs have been started, then stopping initialization
            return;
        }

        Task.Run(async () =>
            {
                var ground = 
                    _localInfoManager.Grounds[groundId] 
                        as FarmerGround;

                if (ground == null)
                {
                    return;
                }

                await AddScheduledPlansToScheduler(ground);
                RunOneShotPlans(ground);
            });
    }

    private void LocalGroundRemoved(object sender, GroundChangedEventArgs e)
    {
        if (!_scheduledPlanJobsByGround.ContainsKey(e.GroundId) ||
            !_scheduledPlanJobsByGround[e.GroundId].Any())
        {
            return;
        }

        _jobScheduler.DeleteJobs(
            _scheduledPlanJobsByGround[e.GroundId]
                .Select(j => j.Key)
                .ToList()
                .AsReadOnly());
    }

    private void RunOneShotPlans(FarmerGround ground)
    {
        if (ground == null) return;
        
        var plans = GetPlansIdToRun(ground);

        EnqueueAndRun(ground.ID, plans);
    }

    private void RunOneShotPlans()
    {
        var plansByGround = GetPlansToRun();
        foreach (var plan in plansByGround)
        {
            EnqueueAndRun(plan.Key, plan.Value);
        }
    }

    private void TryRunNextPlan(string groundId)
    {
        if (!_plansToRunByGround.ContainsKey(groundId) ||
            !_plansToRunByGround[groundId].TryDequeue(out var nextPlanId))
        {
            return;
        }

        SendRunPlan(nextPlanId, groundId);
    }

    private void EnqueueAndRun(string groundId, IEnumerable<string> plans)
    {
        var queue = new ConcurrentQueue<string>(plans);
        _plansToRunByGround.TryAdd(groundId, queue);

        TryRunNextPlan(groundId);
    }

    private IEnumerable<string> GetPlansIdToRun(FarmerGround ground)
    {
        var now = DateTime.UtcNow;

        return 
            ground
                .Plans
                    .Where(x => 
                        (x.ValidFromDt == null || x.ValidFromDt <= now) && // valid start
                        (x.ValidToDt == null || x.ValidToDt > now)) // valid end
                    .Where(x => string.IsNullOrEmpty(x.CronSchedule)) // not scheduled plan
                    .OrderBy(x => x.Priority)
                    .Select(x => x.ID)
                    .ToList();
    }

    private Dictionary<string, IEnumerable<string>> GetPlansToRun()
    {
        var result = new Dictionary<string, IEnumerable<string>>();

        foreach (var gGround in _localInfoManager.Grounds.Values)
        {
            var ground = gGround as FarmerGround;
            if (ground == null) continue;
            
            var plans = new List<string>();

            var plansInGround = GetPlansIdToRun(ground);
            if (plansInGround != null && plansInGround.Any())
            {
                plans.AddRange(plansInGround);
            }

            result.Add(gGround.ID, plans);
        }

        return result;
    }

    private void SendRunPlan(IJobExecutionContext context)
    {
        JobDataMap dataMap = context.MergedJobDataMap; 

        string planId = dataMap.GetString("planId");

        SendRunPlan(planId, GroundUtils.GetGroundByPlan(planId)?.ID);
    }

    private void SendRunPlan(string planId, string groundId)
    {
        SendNewOperation(AppOperation.RunPlan, new [] { planId, groundId });
    }

    private async Task PrepareScheduler()
    {
        StdSchedulerFactory factory = new StdSchedulerFactory();
        _jobScheduler = await factory.GetScheduler();

        if (string.IsNullOrEmpty(PlanCheckSchedule))
        {
            SmartFarmerLog.Information("Not defined plan check schedule. Stopping automatic job");
            return;
        }

        IJobDetail job;
        ITrigger trigger;
        CreatePlanCheckJob(out job, out trigger);

        var listener = new JobExecutionListener();

        listener.CheckPlansToRunRequested += async (s, e) => await AddScheduledPlansToScheduler();
        listener.NewRunPlansRequested += (s, e) => SendRunPlan(e.Context);

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
        foreach (var gGround in _localInfoManager.Grounds.Values)
        {
            await (AddScheduledPlansToScheduler(gGround as FarmerGround));
        }
    }

    private async Task AddScheduledPlansToScheduler(FarmerGround ground)
    {
        if (ground == null) return;
                
        var jobList = _scheduledPlanJobsByGround.ContainsKey(ground.ID) ?
            _scheduledPlanJobsByGround[ground.ID] :
            new List<IJobDetail>();

        _scheduledPlanJobsByGround.TryAdd(ground.ID, jobList);

        _groundProcessingSemaphore.Wait();
        SmartFarmerLog.Debug($"checking plans for ground {ground.ID}");

        var now = DateTime.UtcNow;

        var plansInGround = 
            ground
                .Plans
                    .Where(x => 
                        (x.ValidFromDt == null || x.ValidFromDt <= now) && // valid start
                        (x.ValidToDt == null || x.ValidToDt > now) && // valid end
                        !string.IsNullOrEmpty(x.CronSchedule)) // scheduled plan
                    //TODO .Where(x => IsValidCronSchedule(x.CronSchedule))
                    .OrderBy(x => x.Priority)
                    .ToList();
        
        SmartFarmerLog.Debug($"{plansInGround.Count} plans to be processed");

        foreach (var plan in plansInGround)
        {
            //TODO filter out already processed plans
            if (IsAlreadyBeenEvaluated(ground.ID, plan.ID))
            {
                SmartFarmerLog.Debug($"{plan.ID} is already planned. Skipping");
                continue;
            }

            SmartFarmerLog.Debug($"Scheduling {plan.ID} with cron {plan.CronSchedule}");
            CreateScheduledPlanJob(plan, ground.ID, out var job, out var trigger);

            // Tell Quartz to schedule the job using our trigger
            await _jobScheduler.ScheduleJob(job, trigger);

            jobList.Add(job);
        }

        _groundProcessingSemaphore.Release();
    }

    private bool IsAlreadyBeenEvaluated(string groundId, string planId)
    {
        var jobList = _scheduledPlanJobsByGround[groundId];

        if (jobList.IsNullOrEmpty())
        {
            return false;
        }

        return jobList.Any(x => x.JobDataMap.GetString("planId") == planId);
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

    private void CreateScheduledPlanJob(IFarmerPlan plan, string groundId, out IJobDetail job, out ITrigger trigger)
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