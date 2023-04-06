
using System;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer.Utils;

public class FarmerIrrigationInfoProvider : FarmerServiceLocalProviderBase<IFarmerIrrigationTaskInfo>, IFarmerIrrigationInfoProvider
{
    private static readonly Lazy<FarmerIrrigationInfoProvider> _instance = new(() => new FarmerIrrigationInfoProvider());
    public static FarmerIrrigationInfoProvider Instance => _instance.Value;

    public FarmerIrrigationInfoProvider()
        : base("IrrigationInfo_")
    {
    }
}