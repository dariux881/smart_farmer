using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.DTOs.Tasks;

public class FarmerPlan : IFarmerPlan
{

    public FarmerPlan()
    {
        EditableSteps = new List<IFarmerPlanStep>();
    }

    public string Name { get; set; }

    [JsonIgnore]
    public List<IFarmerPlanStep> EditableSteps { get; protected init; }
    public IReadOnlyList<string> StepIds => EditableSteps.Select(x => x.ID).ToList().AsReadOnly();

    [JsonIgnore]
    public bool IsInProgress { get; private set; }
    [JsonIgnore]
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
                foreach (var step in EditableSteps)
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