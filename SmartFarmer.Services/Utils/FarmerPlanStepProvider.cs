
using System;
using System.Collections.Concurrent;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Utils;

public class FarmerPlanStepProvider : IFarmerPlanStepProvider
{
    private static readonly Lazy<FarmerPlanStepProvider> _instance = new(() => new FarmerPlanStepProvider());
    private ConcurrentDictionary<string, IFarmerPlanStep> _planSteps;

    public static FarmerPlanStepProvider Instance => _instance.Value;

    public FarmerPlanStepProvider()
    {
        _planSteps = new ConcurrentDictionary<string, IFarmerPlanStep>();
    }

    public string GenerateServiceId()
    {
        string id;

        do
        {
            id = "PlanStep_" + StringUtils.RandomString(30);
        } while (_planSteps.ContainsKey(id));

        return id;
    }

    public void AddFarmerService(IFarmerPlanStep plan)
    {
        _planSteps.TryAdd(plan.ID, plan);
    }

    public IFarmerPlanStep GetFarmerService(string planStepId)
    {
        if (_planSteps.TryGetValue(planStepId, out var plan))
        {
            return plan;
        }

        return null;
    }
}