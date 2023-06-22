using System;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks;

namespace SmartFarmer.Services.AI;

public class SmartFarmerAIControllerService : ISmartFarmerAIControllerService
{
    private readonly ISmartFarmerAIControllerServiceProvider _aiProvider;

    public SmartFarmerAIControllerService(ISmartFarmerAIControllerServiceProvider aiProvider)
    {
        _aiProvider = aiProvider;
    }

    public async Task<bool> AnalyseHoverPlanResult(string userId, FarmerHoverPlanResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        var planCheck = await IsValidHoverPlan(userId, result.PlanId);
        if (!planCheck) throw new InvalidOperationException();

        var plantKindId = GetPlantKindByPlantInstance(result.PlantInstanceId);
        
        var aiModule = _aiProvider.GetAIPlantModuleByPlantId(plantKindId);
        if (aiModule == null)
        {
            SmartFarmerLog.Error($"no such AI Module found for plant {plantKindId}");
            return false;
        }

        return await aiModule.Execute(result);
    }

    public async Task<IFarmerHoverPlan> GenerateHoverPlan(string userId, string plantInstanceId)
    {
        var plantKindId = GetPlantKindByPlantInstance(plantInstanceId);
        
        var aiModule = _aiProvider.GetAIPlantModuleByPlantId(plantKindId);
        if (aiModule == null)
        {
            SmartFarmerLog.Error($"no such AI Module found for plant {plantKindId}");
            return null;
        }

        return await aiModule.GenerateHoverPlan(plantKindId);
    }

    private string GetPlantKindByPlantInstance(string plantInstanceId)
    {
        throw new NotImplementedException();
    }

    private async Task<bool> IsValidHoverPlan(string userId, string planId)
    {
        throw new NotImplementedException();
    }
}
