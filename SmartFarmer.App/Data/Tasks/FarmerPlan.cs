using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Helpers;

namespace SmartFarmer.Data.Tasks;

public class FarmerPlan : IFarmerPlan
{
    private List<FarmerWeekDay> _days;

    public FarmerPlan()
    {
        Steps = new List<FarmerPlanStep>();
    }

    [JsonConstructor]
    public FarmerPlan(string farmerDaysMask)
        : this()
    {
        _days = new List<FarmerWeekDay>();

        if (!string.IsNullOrEmpty(farmerDaysMask))
        {
            FillRecurrentTask(farmerDaysMask);
        }
    }

    private void FillRecurrentTask(string farmerDaysMask)
    {
        var dayCounts = 7;
        if (farmerDaysMask.Length != dayCounts) throw new ArgumentOutOfRangeException("invalid data");

        for(int i=0; i<dayCounts; i++)
        {
            var dayAllowed = farmerDaysMask.ElementAt(i) == '1';
            if (!dayAllowed) continue;

            var day = (FarmerWeekDay)i;
            _days.Add(day);
        }
    }

    public string Name { get; set; }

    public int Priority { get; set; }
    public DateTime? ValidFromDt { get; set; }
    public DateTime? ValidToDt { get; set; }

    public IReadOnlyList<FarmerWeekDay> PlannedDays => _days.AsReadOnly();

    public List<FarmerPlanStep> Steps { get; set; }
    public IReadOnlyList<string> StepIds => Steps.Select(x => x.ID).ToList().AsReadOnly();

    public bool IsInProgress { get; private set; }
    public Exception LastException { get; private set; }

    public string ID { get; set; }

    public async Task Execute(CancellationToken token)
    {
        // resetting last exception, related to previous executions
        LastException = null;
        IsInProgress = true;

        // starting new plan execution
        SmartFarmerLog.Information("starting plan \"" + Name + "\"");
        
        try
        {
            foreach (var step in Steps)
            {
                await step.Execute(null, token);
            }
        }
        catch (TaskCanceledException taskCanceled)
        {
            LastException = taskCanceled;
            SmartFarmerLog.Exception(taskCanceled);
        }
        catch (Exception ex)
        {
            LastException = ex;
            SmartFarmerLog.Exception(ex);
        }
        finally
        {
            IsInProgress = false;
            SmartFarmerLog.Information("stopping plan \"" + Name + "\"");
        }
    }
}