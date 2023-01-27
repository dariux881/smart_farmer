
using System;
using System.Collections.Concurrent;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.Utils;

public class FarmerIrrigationInfoProvider : IFarmerIrrigationInfoProvider
{
    private static readonly Lazy<FarmerIrrigationInfoProvider> _instance = new(() => new FarmerIrrigationInfoProvider());
    private ConcurrentDictionary<string, IFarmerIrrigationTaskInfo> _info;

    public static FarmerIrrigationInfoProvider Instance => _instance.Value;

    public FarmerIrrigationInfoProvider()
    {
        _info = new ConcurrentDictionary<string, IFarmerIrrigationTaskInfo>();
    }

    public string GenerateServiceId()
    {
        string id;

        do
        {
            id = "IrrigationInfo_" + StringUtils.RandomString(10);
        } while (_info.ContainsKey(id));

        return id;
    }

    public bool AddFarmerService(IFarmerIrrigationTaskInfo plan)
    {
        return _info.TryAdd(plan.ID, plan);
    }

    public IFarmerIrrigationTaskInfo GetFarmerService(string planId)
    {
        if (_info.TryGetValue(planId, out var plan))
        {
            return plan;
        }

        return null;
    }

}