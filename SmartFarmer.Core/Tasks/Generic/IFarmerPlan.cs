using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Helpers;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic
{

    public interface IFarmerPlan : IHasProgressCheckInfo, IFarmerService
    {
        string Name { get; }

        DateTime? ValidFromDt { get; }
        DateTime? ValidToDt { get; }
        IReadOnlyList<DayOfWeek> PlannedDays { get; }

        // lower values means major priority
        int Priority { get; }

        IReadOnlyList<string> StepIds { get; }
        Task Execute(CancellationToken token);
    }
}
