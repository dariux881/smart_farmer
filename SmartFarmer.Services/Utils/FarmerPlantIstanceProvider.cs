
using System;
using System.Collections.Concurrent;
using SmartFarmer.Misc;
using SmartFarmer.Plants;

namespace SmartFarmer.Utils;

public class FarmerPlantInstanceProvider : IFarmerServiceProvider<IFarmerPlantInstance>
{
    private static readonly Lazy<FarmerPlantInstanceProvider> _instance = new(() => new FarmerPlantInstanceProvider());
    private ConcurrentDictionary<string, IFarmerPlantInstance> _plants;

    public static FarmerPlantInstanceProvider Instance => _instance.Value;

    public FarmerPlantInstanceProvider()
    {
        _plants = new ConcurrentDictionary<string, IFarmerPlantInstance>();
    }

    public string GenerateServiceId()
    {
        string id;

        do
        {
            id = "PlantInstance_" + StringUtils.RandomString(10);
        } while (_plants.ContainsKey(id));

        return id;
    }

    public void AddFarmerService(IFarmerPlantInstance plant)
    {
        _plants.TryAdd(plant.ID, plant);
    }

    public IFarmerPlantInstance GetFarmerService(string plantId)
    {
        if (_plants.TryGetValue(plantId, out var plant))
        {
            return plant;
        }

        return null;
    }
}