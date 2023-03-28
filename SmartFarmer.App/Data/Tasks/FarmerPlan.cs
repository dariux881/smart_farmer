using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Data.Tasks;

public class FarmerPlan : IFarmerPlan
{
    public FarmerPlan()
    {
        Steps = new List<FarmerPlanStep>();
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