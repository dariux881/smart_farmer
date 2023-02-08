using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFarmer.Alerts;
using SmartFarmer.Data;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Services;

public class SmartFarmerGroundControllerService : ISmartFarmerGroundControllerService 
{
    private readonly ISmartFarmerRepository _repository;

    public SmartFarmerGroundControllerService(ISmartFarmerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<IFarmerGround>> GetFarmerGroundByUserIdAsync(string userId)
    {
        return await _repository.GetFarmerGroundByUserIdAsync(userId);
    }
    
    public async Task<IFarmerGround> GetFarmerGroundByIdForUserAsync(string userId, string groundId)
    {
        return await _repository.GetFarmerGroundByIdForUserAsync(userId, groundId);
    }

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

    
    public async Task<IFarmerPlan> GetFarmerPlanByIdForUserAsync(string userId, string planId)
    {
        return await _repository.GetFarmerPlanByIdAsync(planId, userId);
    }
    
    public async Task<IEnumerable<IFarmerPlan>> GetFarmerPlanByIdsForUserAsync(string userId, string[] planIds)
    {
        return await _repository.GetFarmerPlanByIdsAsync(planIds, userId);
    }

    public async Task<IEnumerable<IFarmerPlanStep>> GetFarmerPlanStepByIdsAsync (string[] ids)
    {
        return await _repository.GetFarmerPlanStepByIdsAsync(ids);
    }

    public async Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByGroundIdAsync(string userId, string groundId)
    {
        return await _repository.GetFarmerAlertsByGroundIdAsync(userId, groundId);
    }

    public async Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByIdAsync(string userId, string[] ids)
    {
        return await _repository.GetFarmerAlertsByIdsAsync(userId, ids);
    }
}