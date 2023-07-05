using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Exceptions;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Data.Tasks;

public class FarmerPlan : IFarmerPlan
{
    private IFarmerGarden _garden;

    public FarmerPlan()
    {
        Steps = new List<FarmerPlanStep>();
    }

    public string Name { get; set; }

    public int Priority { get; set; }
    public DateTime? ValidFromDt { get; set; }
    public DateTime? ValidToDt { get; set; }

    public string CronSchedule { get; set; }

    public List<FarmerPlanStep> Steps { get; set; }
    
    public IReadOnlyList<string> StepIds => Steps.Select(x => x.ID).ToList().AsReadOnly();

    public bool IsInProgress { get; private set; }
    public Exception LastException { get; private set; }

    public string ID { get; set; }

    public IFarmerPlan PropagateGarden(IFarmerGarden garden)
    {
        _garden = garden;

        Steps.ForEach(step => step.PropagateGarden(garden));
        
        return this;
    }

    public async Task<IFarmerPlanExecutionResult> Execute(CancellationToken token)
    {
        var result = new FarmerPlanExecutionResult();

        result.PlanId = ID;

        // resetting last exception, related to previous executions
        LastException = null;
        IsInProgress = true;

        // starting new plan execution
        SmartFarmerLog.Information($"starting plan \"{Name}\"");
        
        try
        {
            foreach (var step in Steps)
            {
                var taskResult = await step.Execute(null, token);

                if (taskResult != null)
                {
                    result.StepResults.Add(
                        new FarmerStepExecutionResult()
                        {
                            StepId = step.ID,
                            TaskInterfaceFullName = step.TaskInterfaceFullName,
                            PlantInstanceId = GetPlantInstanceId(step)
                        });
                }
            }
        }
        catch (FarmerBaseException fEx)
        {
            await SmartFarmerLog.Exception(
                fEx, 
                new SmartFarmer.Alerts.FarmerAlertRequestData()
                {
                    Code = fEx.Code,
                    GardenId = _garden.ID,
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
            await SmartFarmerLog.Exception(
                ex, 
                new SmartFarmer.Alerts.FarmerAlertRequestData()
                {
                    Code = SmartFarmer.Alerts.AlertCode.Unknown,
                    GardenId = _garden.ID,
                    Message = ex.Message,
                    Level = SmartFarmer.Alerts.AlertLevel.Error,
                    Severity = SmartFarmer.Alerts.AlertSeverity.High,
                });
            throw;
        }
        finally
        {
            IsInProgress = false;
        }

        if (LastException != null)
        {
            SmartFarmerLog.Error($"Plan \"{Name}\" stopped with errors");
        }
        else
        {
            SmartFarmerLog.Information($"Plan \"{Name}\" completed");
        }

        result.LastException = LastException;

        return result;
    }

    private string GetPlantInstanceId(FarmerPlanStep step)
    {
        var attributeName = nameof(IHasPlantInstanceReference.PlantInstanceID);

        if (step.BuildParameters != null && 
            step.BuildParameters.ContainsKey(attributeName))
        {
            return step.BuildParameters[attributeName];
        }

        return null;
    }
}