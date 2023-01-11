
using System;
using System.Collections.Concurrent;
using SmartFarmer.Plants;

namespace SmartFarmer.Utils;

public class FarmerPlantProvider
{
    private static readonly Lazy<FarmerPlantProvider> _instance = new(() => new FarmerPlantProvider());
    private ConcurrentDictionary<string, IFarmerPlant> _plants;

    public static FarmerPlantProvider Instance => _instance.Value;

    public FarmerPlantProvider()
    {
        _plants = new ConcurrentDictionary<string, IFarmerPlant>();
    }

    public void AddFarmerPlant(IFarmerPlant plant)
    {
        _plants.TryAdd(plant.ID, plant);
    }

    public IFarmerPlant GetFarmerPlant(string plantId)
    {
        if (_plants.TryGetValue(plantId, out var plant))
        {
            return plant;
        }

        return null;
    }
}