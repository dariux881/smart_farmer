
using System;
using System.Collections.Concurrent;
using SmartFarmer.Misc;
using SmartFarmer.Plants;

namespace SmartFarmer.Utils;

public class FarmerPlantProvider : IFarmerPlantProvider
{
    private static readonly Lazy<FarmerPlantProvider> _instance = new(() => new FarmerPlantProvider());
    private ConcurrentDictionary<string, IFarmerPlant> _plants;

    public static FarmerPlantProvider Instance => _instance.Value;

    public FarmerPlantProvider()
    {
        _plants = new ConcurrentDictionary<string, IFarmerPlant>();
    }

    public string GenerateServiceId()
    {
        string id;

        do
        {
            id = "Plant_" + StringUtils.RandomString(10);
        } while (_plants.ContainsKey(id));

        return id;
    }

    public void AddFarmerService(IFarmerPlant plant)
    {
        _plants.TryAdd(plant.ID, plant);
    }

    public IFarmerPlant GetFarmerService(string plantId)
    {
        if (_plants.TryGetValue(plantId, out var plant))
        {
            return plant;
        }

        return null;
    }
}