using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartFarmer.Exceptions;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Data.Tasks;

public class FarmerPlan : IFarmerPlan
{
    private List<DayOfWeek> _days;
    private IFarmerGround _ground;

    public FarmerPlan()
    {
        Steps = new List<FarmerPlanStep>();
    }

    [JsonConstructor]
    public FarmerPlan(string farmerDaysMask)
        : this()
    {
        _days = new List<DayOfWeek>();

        if (!string.IsNullOrEmpty(farmerDaysMask))
        {
            FillRecurrentTask(farmerDaysMask);
        }
    }

    public string Name { get; set; }

    public int Priority { get; set; }
    public DateTime? ValidFromDt { get; set; }
    public DateTime? ValidToDt { get; set; }

    public IReadOnlyList<DayOfWeek> PlannedDays => _days.AsReadOnly();

    public List<FarmerPlanStep> Steps { get; set; }
    
    public IReadOnlyList<string> StepIds => Steps.Select(x => x.ID).ToList().AsReadOnly();

    public bool IsInProgress { get; private set; }
    public Exception LastException { get; private set; }

    public string ID { get; set; }

    public IFarmerPlan PropagateGround(IFarmerGround ground)
    {
        _ground = ground;

        Steps.ForEach(step => step.PropagateGround(ground));
        
        return this;
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
        catch (FarmerBaseException fEx)
        {
            SmartFarmerLog.Exception(
                fEx, 
                new SmartFarmer.Alerts.FarmerAlertRequestData()
                {
                    Code = fEx.Code,
                    FarmerGroundId = _ground.ID,
                    Message = fEx.Message,
                    Level = fEx.Level,
                    Severity = fEx.Severity,
                    RaisedByTaskId = fEx.RaisedByTaskId,
                    PlantInstanceId = fEx.PlantId
                });
        }
        catch (Exception ex)
        {
            LastException = ex;
            SmartFarmerLog.Exception(
                ex, 
                new SmartFarmer.Alerts.FarmerAlertRequestData()
                {
                    Code = SmartFarmer.Alerts.AlertCode.Unknown,
                    FarmerGroundId = _ground.ID,
                    Message = ex.Message,
                    Level = SmartFarmer.Alerts.AlertLevel.Error,
                    Severity = SmartFarmer.Alerts.AlertSeverity.High,
                });
            throw;
        }
        finally
        {
            IsInProgress = false;
            SmartFarmerLog.Information("stopping plan \"" + Name + "\"");
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

            var day = (DayOfWeek)i;
            _days.Add(day);
        }
    }
}