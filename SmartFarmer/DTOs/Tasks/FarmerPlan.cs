using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Helpers;

namespace SmartFarmer.DTOs.Tasks;

public class FarmerPlan : IFarmerPlan
{
    private string _farmerDaysMask;
    private List<DayOfWeek> _days;

    public FarmerPlan()
    {
        Steps = new List<FarmerPlanStep>();
        _days = new List<DayOfWeek>();
    }

    public string Name { get; set; }

    public List<FarmerPlanStep> Steps { get; set; }
    public IReadOnlyList<string> StepIds => Steps.Select(x => x.ID).ToList().AsReadOnly();

    [JsonIgnore]
    public FarmerGround Ground { get; set; }
    [JsonIgnore]
    public string GroundId { get; set; }

    [JsonIgnore]
    public bool IsInProgress { get; private set; }
    [JsonIgnore]
    public Exception LastException { get; private set; }

    public string ID { get; set; }

    public int Priority { get; set; }
    public DateTime? ValidFromDt { get; set; }
    public DateTime? ValidToDt { get; set; }

    [JsonIgnore]
    public IReadOnlyList<DayOfWeek> PlannedDays => _days.AsReadOnly();

    public string FarmerDaysMask 
    {
        get => _farmerDaysMask;
        set {
            if (value == null) 
            {
                _days.Clear();
                return;
            }

            _farmerDaysMask = value;
            var dayCounts = 7;
            if (_farmerDaysMask.Length != dayCounts) throw new ArgumentOutOfRangeException("invalid data");

            for(int i=0; i<dayCounts; i++)
            {
                var dayAllowed = _farmerDaysMask.ElementAt(i) == '1';
                if (!dayAllowed) continue;

                var day = (DayOfWeek)i;
                _days.Add(day);
            }
        }
    }

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