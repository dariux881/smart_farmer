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
    private ConcurrentDictionary<string, List<IJobDetail>> _scheduledPlanJobsByGarden;
    private ConcurrentDictionary<string, ConcurrentQueue<string>> _plansToRunByGarden;
    private SemaphoreSlim _gardenProcessingSemaphore;
    private const string CHECK_PLAN_GROUP = "checkPlanGroup";
    private const string SCHEDULED_PLAN_GROUP = "scheduledPlanGroup";
    private readonly IFarmerLocalInformationManager _localInfoManager;
    private readonly IFarmerAppCommunicationHandler _communicationManager;
    private CancellationToken _operationsToken;

    public AutomaticOperationalManager(AppConfiguration appConfiguration)
    {
        _scheduledPlanJobsByGarden = new ConcurrentDictionary<string, List<IJobDetail>>();
        _plansToRunByGarden = new ConcurrentDictionary<string, ConcurrentQueue<string>>();

        _gardenProcessingSemaphore = new SemaphoreSlim(1);
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
            _communicationManager.LocalGardenAdded -= LocalGardenAdded;
            _communicationManager.LocalGardenRemoved -= LocalGardenRemoved;
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

            // present gardens have been already processed. Subscribing to gardens change
            _communicationManager.LocalGardenAdded += LocalGardenAdded;
            _communicationManager.LocalGardenRemoved += LocalGardenRemoved;

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

        TryRunNextPlan(GardenUtils.GetGardenByPlan(args.AdditionalData.First())?.ID);
    }

    private void LocalGardenAdded(object sender, GardenChangedEventArgs e)
    {
        var gardenId = e.GardenId;

        if (_scheduledPlanJobsByGarden.ContainsKey(gardenId))
        {
            // garden has been already processed. Recurrent jobs have been started, then stopping initialization
            return;
        }

        Task.Run(async () =>
            {
                var garden = 
                    _localInfoManager.Gardens[gardenId] 
                        as FarmerGarden;

                if (garden == null)
                {
                    return;
                }

                await AddScheduledPlansToScheduler(garden);
                RunOneShotPlans(garden);
            });
    }

    private void LocalGardenRemoved(object sender, GardenChangedEventArgs e)
    {
        if (!_scheduledPlanJobsByGarden.ContainsKey(e.GardenId) ||
            !_scheduledPlanJobsByGarden[e.GardenId].Any())
        {
            return;
        }

        _jobScheduler.DeleteJobs(
            _scheduledPlanJobsByGarden[e.GardenId]
                .Select(j => j.Key)
                .ToList()
                .AsReadOnly());
    }

    private void RunOneShotPlans(FarmerGarden garden)
    {
        if (garden == null) return;
        
        var plans = GetPlansIdToRun(garden);

        EnqueueAndRun(garden.ID, plans);
    }

    private void RunOneShotPlans()
    {
        var plansByGarden = GetPlansToRun();
        foreach (var plan in plansByGarden)
        {
            EnqueueAndRun(plan.Key, plan.Value);
        }
    }

    private void TryRunNextPlan(string gardenId)
    {
        if (!_plansToRunByGarden.ContainsKey(gardenId) ||
            !_plansToRunByGarden[gardenId].TryDequeue(out var nextPlanId))
        {
            return;
        }

        SendRunPlan(nextPlanId, gardenId);
    }

    private void EnqueueAndRun(string gardenId, IEnumerable<string> plans)
    {
        var queue = new ConcurrentQueue<string>(plans);
        _plansToRunByGarden.TryAdd(gardenId, queue);

        TryRunNextPlan(gardenId);
    }

    private IEnumerable<string> GetPlansIdToRun(FarmerGarden garden)
    {
        var now = DateTime.UtcNow;

        return 
            garden
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

        foreach (var gGarden in _localInfoManager.Gardens.Values)
        {
            var garden = gGarden as FarmerGarden;
            if (garden == null) continue;
            
            var plans = new List<string>();

            var plansInGarden = GetPlansIdToRun(garden);
            if (plansInGarden != null && plansInGarden.Any())
            {
                plans.AddRange(plansInGarden);
            }

            result.Add(gGarden.ID, plans);
        }

        return result;
    }

    private void SendRunPlan(IJobExecutionContext context)
    {
        JobDataMap dataMap = context.MergedJobDataMap; 

        string planId = dataMap.GetString("planId");

        SendRunPlan(planId, GardenUtils.GetGardenByPlan(planId)?.ID);
    }

    private void SendRunPlan(string planId, string gardenId)
    {
        SendNewOperation(AppOperation.RunPlan, new [] { planId, gardenId });
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
        foreach (var gGarden in _localInfoManager.Gardens.Values)
        {
            await (AddScheduledPlansToScheduler(gGarden as FarmerGarden));
        }
    }

    private async Task AddScheduledPlansToScheduler(FarmerGarden garden)
    {
        if (garden == null) return;
                
        var jobList = _scheduledPlanJobsByGarden.ContainsKey(garden.ID) ?
            _scheduledPlanJobsByGarden[garden.ID] :
            new List<IJobDetail>();

        _scheduledPlanJobsByGarden.TryAdd(garden.ID, jobList);

        _gardenProcessingSemaphore.Wait();
        SmartFarmerLog.Debug($"checking plans for garden {garden.ID}");

        var now = DateTime.UtcNow;

        var plansInGarden = 
            garden
                .Plans
                    .Where(x => 
                        (x.ValidFromDt == null || x.ValidFromDt <= now) && // valid start
                        (x.ValidToDt == null || x.ValidToDt > now) && // valid end
                        !string.IsNullOrEmpty(x.CronSchedule)) // scheduled plan
                    //TODO .Where(x => IsValidCronSchedule(x.CronSchedule))
                    .OrderBy(x => x.Priority)
                    .ToList();
        
        SmartFarmerLog.Debug($"{plansInGarden.Count} plans to be processed");

        foreach (var plan in plansInGarden)
        {
            //TODO filter out already processed plans
            if (IsAlreadyBeenEvaluated(garden.ID, plan.ID))
            {
                SmartFarmerLog.Debug($"{plan.ID} is already planned. Skipping");
                continue;
            }

            SmartFarmerLog.Debug($"Scheduling {plan.ID} with cron {plan.CronSchedule}");
            CreateScheduledPlanJob(plan, garden.ID, out var job, out var trigger);

            // Tell Quartz to schedule the job using our trigger
            await _jobScheduler.ScheduleJob(job, trigger);

            jobList.Add(job);
        }

        _gardenProcessingSemaphore.Release();
    }

    private bool IsAlreadyBeenEvaluated(string gardenId, string planId)
    {
        var jobList = _scheduledPlanJobsByGarden[gardenId];

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

    private void CreateScheduledPlanJob(IFarmerPlan plan, string gardenId, out IJobDetail job, out ITrigger trigger)
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