using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic;

public interface IFarmerBasicPlan : IFarmerService
{
    string Name { get; }
    IReadOnlyList<string> StepIds { get; }
    
    Task<IFarmerPlanExecutionResult> Execute(CancellationToken token);
    Exception LastException { get; }
}
