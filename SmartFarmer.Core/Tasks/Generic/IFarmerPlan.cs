using System;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic;

public interface IFarmerPlan : IFarmerBasicPlan
{
    DateTime? ValidFromDt { get; }
    DateTime? ValidToDt { get; }
    string CronSchedule { get; }

    // lower values means major priority
    int Priority { get; }
}
