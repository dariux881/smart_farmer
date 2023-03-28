
using System;
using SmartFarmer.Plants;

namespace SmartFarmer.Utils;

public class FarmerPlantInstanceProvider : FarmerServiceLocalProviderBase<IFarmerPlantInstance>, IFarmerPlantInstanceProvider
{
    private static readonly Lazy<FarmerPlantInstanceProvider> _instance = new(() => new FarmerPlantInstanceProvider());
    public static FarmerPlantInstanceProvider Instance => _instance.Value;

    public FarmerPlantInstanceProvider()
        : base("PlantInstance_")
    {
    }
}