using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic;

public interface IFarmerPlanStep : IHasProgressCheckInfo, IHasTaskInterfaceFullName, IFarmerService 
{
    string TaskClassFullName { get; }

    TimeSpan Delay { get; }
    IDictionary<string, string> BuildParameters { get; }

    Task<object> Execute(IDictionary<string, string> parameters, CancellationToken token);
}
