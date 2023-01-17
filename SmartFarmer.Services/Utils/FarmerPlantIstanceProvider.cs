
using System;
using System.Collections.Concurrent;
using SmartFarmer.Plants;

namespace SmartFarmer.Utils;

public class FarmerPlantInstanceProvider
{
    private static readonly Lazy<FarmerPlantInstanceProvider> _instance = new(() => new FarmerPlantInstanceProvider());
    private ConcurrentDictionary<string, IFarmerPlantInstance> _plants;

    public static FarmerPlantInstanceProvider Instance => _instance.Value;

    public FarmerPlantInstanceProvider()
    {
        _plants = new ConcurrentDictionary<string, IFarmerPlantInstance>();
    }

    public void AddFarmerPlantInstance(IFarmerPlantInstance plant)
    {
        _plants.TryAdd(plant.ID, plant);
    }

    public IFarmerPlantInstance GetFarmerPlantInstance(string plantId)
    {
        if (_plants.TryGetValue(plantId, out var plant))
        {
            return plant;
        }

        return null;
    }
}