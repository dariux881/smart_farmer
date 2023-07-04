using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Misc;
using SmartFarmer.Services.Plant;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Services.AI;

public class SmartFarmerAIControllerService : ISmartFarmerAIControllerService
{
    private readonly ISmartFarmerAIControllerServiceProvider _aiProvider;
    private readonly ConcurrentDictionary<string, HashSet<string>> _plansByUser;
    private readonly ISmartFarmerPlantControllerService _plantService;

    public SmartFarmerAIControllerService(
        ISmartFarmerAIControllerServiceProvider aiProvider,
        ISmartFarmerPlantControllerService plantService)
    {
        _plansByUser = new ConcurrentDictionary<string, HashSet<string>>();

        _aiProvider = aiProvider;
        _plantService = plantService;
    }

    public bool IsValidHoverPlan(string userId, string planId)
    {
        if (!_plansByUser.ContainsKey(userId))
        {
            return false;
        }

        return _plansByUser[userId].Contains(planId);
    }

    public async Task<bool> AnalyseHoverPlanResult(
        string userId, 
        FarmerPlan plan,
        IFarmerPlanExecutionResult result)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        if (result == null) throw new ArgumentNullException(nameof(result));

        if (plan.ID != result.PlanId) throw new InvalidOperationException($"plan result is not related to given plan");

        var planCheck = IsValidHoverPlan(userId, result.PlanId);
        if (!planCheck) throw new InvalidOperationException();

        await AnalysePlanCore(userId, plan, result);
        await RemoveStoredPlan(userId, result.PlanId);

        return true;
    }

    public async Task<IFarmerPlan> GenerateHoverPlan(string userId, string plantInstanceId)
    {
        var plantInstance = await GetPlantInstance(plantInstanceId, userId);
        
        var aiModule = _aiProvider.GetAIModuleByPlant(plantInstance);
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

    private async Task<FarmerPlantInstance> GetPlantInstance(string plantInstanceId, string userId)
    {
        return await _plantService.GetFarmerPlantInstanceByIdForUserAsync(userId, plantInstanceId) as FarmerPlantInstance;
    }

    private async Task StoreHoverPlan(string userId, IFarmerPlan hoverPlan)
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

    private async Task RemoveStoredPlan(string userId, string planId)
    {
        await Task.CompletedTask;

        if (!_plansByUser.TryGetValue(userId, out var plans))
        {
            return;
        }

        plans.Remove(planId);
    }

    private async Task<FarmerAIDetectionLog> AnalysePlanCore(string userId, FarmerPlan plan, IFarmerPlanExecutionResult result)
    {
        FarmerAIDetectionLog log = new FarmerAIDetectionLog();

        if (!result.IsSuccess)
        {
            SmartFarmerLog.Error(result.ErrorMessage);

            log.Messages.Add(
                new FarmerAIDetectionLogMessage()
                {
                    Level = LogMessageLevel.Error,
                    Message = result.ErrorMessage,
                    RequiresAction = false
                });
                
            return log;
        }

        var stepsWithResult = plan.Steps.Where(x => result.TaskResults.ContainsKey(x.ID));

        if (!stepsWithResult.Any())
        {
            return log;
        }

        foreach (var step in stepsWithResult)
        {
            SmartFarmerLog.Debug($"processing step {step.ID} on task {step.TaskInterfaceFullName}");
            FarmerAIDetectionLog detectionLog = null;

            // check if refers to PlantInstance
            if (HasPlantInstance(step, out var plantInstanceId))
            {
                detectionLog = await AnalysePlantBasedStep(
                    userId, 
                    step.ID, 
                    plantInstanceId, 
                    result.TaskResults[step.ID]);
            }
            else
            {
                detectionLog = await AnalyseTaskBasedStep(
                    userId, 
                    step.ID, 
                    step.TaskInterfaceFullName, 
                    result.TaskResults[step.ID]);
            }

            if (detectionLog != null)
            {
                log.InjectFrom(detectionLog);
            }
        }

        await Task.CompletedTask;
        return log;
    }

    private async Task<FarmerAIDetectionLog> AnalysePlantBasedStep(
        string userId, 
        string stepId,
        string plantInstanceId, 
        object stepData)
    {
        
        var plantInstance = await GetPlantInstance(plantInstanceId, userId);
        
        var aiModule = _aiProvider.GetAIModuleByPlant(plantInstance);
        if (aiModule == null)
        {
            SmartFarmerLog.Error($"no such AI Module found for plant {plantInstance.ID} / {plantInstance.PlantKindID}");

            return 
                new FarmerAIDetectionLog(
                    new FarmerAIDetectionLogMessage()
                    {
                        Level = LogMessageLevel.Error,
                        Message = $"no such AI Module found for plant {plantInstance.ID} / {plantInstance.PlantKindID}",
                        StepID = stepId
                    });
        }

        return await aiModule.ExecuteDetection(stepData);
    }

    private async Task<FarmerAIDetectionLog> AnalyseTaskBasedStep(
        string userId, 
        string stepId,
        string taskInterfaceFullName, 
        object stepData)
    {
        var aiModule = _aiProvider.GetAITaskModuleByTask(taskInterfaceFullName);
        if (aiModule == null)
        {
            SmartFarmerLog.Error($"no such AI Module found for task {taskInterfaceFullName}");
            
            return 
                new FarmerAIDetectionLog(
                    new FarmerAIDetectionLogMessage()
                    {
                        Level = LogMessageLevel.Error,
                        Message = $"no such AI Module found for task {taskInterfaceFullName}",
                        StepID = stepId
                    });
        }

        return await aiModule.ExecuteDetection(stepData);
    }

    private bool HasPlantInstance(FarmerPlanStep step, out string plantInstanceId)
    {
        plantInstanceId = null;
        var attributeName = nameof(IHasPlantInstanceReference.PlantInstanceID);

        if (step.BuildParameters.ContainsKey(attributeName))
        {
            plantInstanceId = step.BuildParameters[attributeName];
            return true;
        }

        return false;
    }
}
