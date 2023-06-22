using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks;

namespace SmartFarmer.DTOs.Tasks;

public class FarmerHoverPlan : IFarmerHoverPlan
{
    public string Name { get; set; }

    public List<FarmerPlanStep> Steps { get; set; }
    public IReadOnlyList<string> StepIds => Steps.Select(x => x.ID).ToList().AsReadOnly();

    public string ID => throw new NotImplementedException();

    public Task Execute(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}