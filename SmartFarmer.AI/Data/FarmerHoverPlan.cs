using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Data;

public class FarmerHoverPlan : IFarmerPlan
{
    public FarmerHoverPlan()
    {
        Steps = new List<FarmerHoverPlanStep>();
    }
    
    public string Name { get; set; }

    public List<FarmerHoverPlanStep> Steps { get; set; }
    public IReadOnlyList<string> StepIds => Steps.Select(x => x.ID).ToList().AsReadOnly();

    public string ID { get; set; }

    public DateTime? ValidFromDt { get; set; }

    public DateTime? ValidToDt { get; set; }

    public string CronSchedule { get; set; }

    public int Priority { get; set; }
    public Exception LastException { get; set; }

    public Task<FarmerPlanExecutionResult> Execute(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}