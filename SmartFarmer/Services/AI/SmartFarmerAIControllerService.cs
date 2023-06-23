using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.Misc;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;

namespace SmartFarmer.Services.AI;

public class SmartFarmerAIControllerService : ISmartFarmerAIControllerService
{
    private readonly ISmartFarmerAIControllerServiceProvider _aiProvider;
    private readonly ConcurrentDictionary<string, HashSet<string>> _plansByUser;
    private readonly ISmartFarmerRepository _repository;

    public SmartFarmerAIControllerService(
        ISmartFarmerAIControllerServiceProvider aiProvider,
        ISmartFarmerRepository repository)
    {
        _plansByUser = new ConcurrentDictionary<string, HashSet<string>>();

        _aiProvider = aiProvider;
        _repository = repository;
    }

    public async Task<bool> AnalyseHoverPlanResult(string userId, FarmerHoverPlanResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        var planCheck = await IsValidHoverPlan(userId, result.PlanId);
        if (!planCheck) throw new InvalidOperationException();

        var plantInstance = await GetPlantInstance(result.PlantInstanceId, userId);
        
        var aiModule = _aiProvider.GetAIPlantModuleByPlantId(plantInstance.ID, plantInstance.PlantKindID);
        if (aiModule == null)
        {
            SmartFarmerLog.Error($"no such AI Module found for plant {plantInstance.ID} / {plantInstance.PlantKindID}");
            return false;
        }

        await RemoveStoredPlan(userId, result.PlanId);

        return await aiModule.StartDetection(result);
    }

    public async Task<IFarmerHoverPlan> GenerateHoverPlan(string userId, string plantInstanceId)
    {
        var plantInstance = await GetPlantInstance(plantInstanceId, userId);
        
        var aiModule = _aiProvider.GetAIPlantModuleByPlantId(plantInstance.ID, plantInstance.PlantKindID);
        if (aiModule == null)
        {
            SmartFarmerLog.Error($"no such AI Module found for plant {plantInstance}");
            return null;
        }

        var hoverPlan = await aiModule.GenerateHoverPlan(plantInstance);

        if (hoverPlan == null)
        {
            return hoverPlan;
        }

        await StoreHoverPlan(userId, hoverPlan);

        return hoverPlan;
    }

    private async Task<IFarmerPlantInstance> GetPlantInstance(string plantInstanceId, string userId)
    {
        return await _repository.GetFarmerPlantInstanceById(plantInstanceId, userId);
    }

    private async Task StoreHoverPlan(string userId, IFarmerHoverPlan hoverPlan)
    {
        HashSet<string> plans = null;

        if (!_plansByUser.TryGetValue(userId, out plans))
        {
            plans = new HashSet<string>();
        }

        plans.Add(hoverPlan.ID);

        _plansByUser.TryAdd(userId, plans);

        //TODO evaluate to store plans in DB
        await Task.CompletedTask;
    }

    private async Task<bool> IsValidHoverPlan(string userId, string planId)
    {
        await Task.CompletedTask;

        if (!_plansByUser.TryGetValue(userId, out var plans))
        {
            return false;
        }

        return plans.Any(x => x == planId);
    }

    private async Task RemoveStoredPlan(string userId, string planId)
    {
        await Task.CompletedTask;

        if (!_plansByUser.TryGetValue(userId, out var plans))
        {
            return;
        }

        plans.Remove(planId);
    }
}
