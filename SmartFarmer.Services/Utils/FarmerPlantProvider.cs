
using System;
using SmartFarmer.Plants;

namespace SmartFarmer.Utils;

public class FarmerPlantProvider : FarmerServiceLocalProviderBase<IFarmerPlant>,  IFarmerPlantProvider
{
    private static readonly Lazy<FarmerPlantProvider> _instance = new(() => new FarmerPlantProvider());
    public static FarmerPlantProvider Instance => _instance.Value;

    public FarmerPlantProvider()
        : base("Plant_")
    {
    }
}