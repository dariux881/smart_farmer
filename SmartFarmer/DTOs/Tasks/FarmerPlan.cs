using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
//using Newtonsoft.Json;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks;

namespace SmartFarmer.DTOs.Tasks;

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
    public FarmerGarden Garden { get; set; }
    [JsonIgnore]
    public string GardenId { get; set; }

    [JsonIgnore]
    public bool IsInProgress { get; private set; }
    [JsonIgnore]
    public Exception LastException { get; private set; }

    public string ID { get; set; }

    public int Priority { get; set; }
    public DateTime? ValidFromDt { get; set; }
    public DateTime? ValidToDt { get; set; }
    public string CronSchedule { get; set; }

    public Task<IFarmerPlanExecutionResult> Execute(CancellationToken token)
    {
        throw new InvalidOperationException();
    }
}