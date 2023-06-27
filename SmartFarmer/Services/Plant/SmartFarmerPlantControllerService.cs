using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.Plants;

namespace SmartFarmer.Services.Plant;

public class SmartFarmerPlantControllerService : ISmartFarmerPlantControllerService 
{
    private readonly ISmartFarmerRepository _repository;

    public SmartFarmerPlantControllerService(ISmartFarmerRepository repository)
    {
        _repository = repository;
    }

    public event EventHandler<NewPlantEventArgs> NewPlantInGarden;

    public async Task<IFarmerPlantInstance> GetFarmerPlantInstanceByIdForUserAsync(string userId, string plantId)
    {
        return await _repository.GetFarmerPlantInstanceById(plantId, userId);
    }
    
    public async Task<IEnumerable<IFarmerPlantInstance>> GetFarmerPlantInstanceByIdsForUserAsync(string userId, string[] plantIds)
    {
        return await _repository.GetFarmerPlantsInstanceById(plantIds, userId);
    }

    public async Task<IFarmerPlant> GetFarmerPlantByIdAsync(string plantId)
    {
        return await _repository.GetFarmerPlantById(plantId);
    }
    
    public async Task<IEnumerable<IFarmerPlant>> GetFarmerPlantByIdsAsync(string[] plantIds)
    {
        return await _repository.GetFarmerPlantsById(plantIds);
    }

    public async Task<IrrigationHistory> GetFarmerIrrigationHistoryByPlantAsync(string userId, string plantId)
    {
        return await _repository.GetFarmerIrrigationHistoryByPlant(plantId, userId);
    }
    
    public async Task<bool> MarkIrrigationInstance(string userId, FarmerPlantIrrigationInstance irrigationInstance)
    {
        return await _repository.MarkIrrigationInstance(irrigationInstance, userId);
    }

    public async Task<bool> AddFarmerPlantInstance(string userId, FarmerPlantRequestData data)
    {
        var newPlantId = await _repository.AddFarmerPlantInstance(userId, data);

        if (!string.IsNullOrEmpty(newPlantId))
        {
            NewPlantInGarden?.Invoke(this, new NewPlantEventArgs(data.GardenId, newPlantId));
        }

        return newPlantId != null;
    }
}