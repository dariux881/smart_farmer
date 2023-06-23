using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.Misc;
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

        var plantKindId = await GetPlantKindByPlantInstance(result.PlantInstanceId, userId);
        
        var aiModule = _aiProvider.GetAIPlantModuleByPlantId(plantKindId);
        if (aiModule == null)
        {
            SmartFarmerLog.Error($"no such AI Module found for plant {plantKindId}");
            return false;
        }

        await RemoveStoredPlan(userId, result.PlanId);

        return await aiModule.Execute(result);
    }

    public async Task<IFarmerHoverPlan> GenerateHoverPlan(string userId, string plantInstanceId)
    {
        var plantKindId = await GetPlantKindByPlantInstance(plantInstanceId, userId);
        
        var aiModule = _aiProvider.GetAIPlantModuleByPlantId(plantKindId);
        if (aiModule == null)
        {
            SmartFarmerLog.Error($"no such AI Module found for plant {plantKindId}");
            return null;
        }

        var hoverPlan = await aiModule.GenerateHoverPlan(plantKindId);

        if (hoverPlan == null)
        {
            return hoverPlan;
        }

        await StoreHoverPlan(userId, hoverPlan);

        return hoverPlan;
    }

    private async Task<string> GetPlantKindByPlantInstance(string plantInstanceId, string userId)
    {
        var plantInstance = await _repository.GetFarmerPlantInstanceById(plantInstanceId, userId);

        return plantInstance?.PlantKindID;
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
