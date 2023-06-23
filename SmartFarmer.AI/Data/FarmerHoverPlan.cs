using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks;

namespace SmartFarmer.Data;

public class FarmerHoverPlan : IFarmerHoverPlan
{
    public FarmerHoverPlan()
    {
        Steps = new List<FarmerHoverPlanStep>();
    }
    
    public string Name { get; set; }

    public List<FarmerHoverPlanStep> Steps { get; set; }
    public IReadOnlyList<string> StepIds => Steps.Select(x => x.ID).ToList().AsReadOnly();

    public string ID { get; set; }
    public Task Execute(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}