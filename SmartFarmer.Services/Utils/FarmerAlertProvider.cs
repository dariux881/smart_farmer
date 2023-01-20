
using System;
using System.Collections.Concurrent;
using SmartFarmer.Alerts;
using SmartFarmer.Misc;

namespace SmartFarmer.Utils;

public class FarmerAlertProvider : IFarmerAlertProvider
{
    private static readonly Lazy<FarmerAlertProvider> _instance = new(() => new FarmerAlertProvider());
    private ConcurrentDictionary<string, IFarmerAlert> _alerts;

    public static FarmerAlertProvider Instance => _instance.Value;

    public FarmerAlertProvider()
    {
        _alerts = new ConcurrentDictionary<string, IFarmerAlert>();
    }

    public string GenerateServiceId()
    {
        string id;

        do
        {
            id = "Alert_" + StringUtils.RandomString(10);
        } while (_alerts.ContainsKey(id));

        return id;
    }

    public void AddFarmerService(IFarmerAlert alert)
    {
        _alerts.TryAdd(alert.ID, alert);
    }

    public IFarmerAlert GetFarmerService(string alertId)
    {
        if (_alerts.TryGetValue(alertId, out var alert))
        {
            return alert;
        }

        return null;
    }

}