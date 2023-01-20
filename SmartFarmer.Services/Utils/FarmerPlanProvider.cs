
using System;
using System.Collections.Concurrent;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Utils;

public class FarmerPlanProvider : IFarmerPlanProvider
{
    private static readonly Lazy<FarmerPlanProvider> _instance = new(() => new FarmerPlanProvider());
    private ConcurrentDictionary<string, IFarmerPlan> _plans;

    public static FarmerPlanProvider Instance => _instance.Value;

    public FarmerPlanProvider()
    {
        _plans = new ConcurrentDictionary<string, IFarmerPlan>();
    }

    public string GenerateServiceId()
    {
        string id;

        do
        {
            id = "Plan_" + StringUtils.RandomString(10);
        } while (_plans.ContainsKey(id));

        return id;
    }

    public void AddFarmerService(IFarmerPlan plan)
    {
        _plans.TryAdd(plan.ID, plan);
    }

    public IFarmerPlan GetFarmerService(string planId)
    {
        if (_plans.TryGetValue(planId, out var plan))
        {
            return plan;
        }

        return null;
    }

}