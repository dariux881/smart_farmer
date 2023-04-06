
using System;
using SmartFarmer.Alerts;

namespace SmartFarmer.Utils;

public class FarmerAlertProvider : FarmerServiceLocalProviderBase<IFarmerAlert>, IFarmerAlertProvider
{
    private static readonly Lazy<FarmerAlertProvider> _instance = new(() => new FarmerAlertProvider());
    public static FarmerAlertProvider Instance => _instance.Value;

    public FarmerAlertProvider()
        : base("Alert_")
    {
    }
}