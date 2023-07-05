using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.Data;
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
    private readonly ISmartFarmerPlantControllerService _plantService;
    private readonly ISmartFarmerRepository _repository;

    public SmartFarmerAIControllerService(
        ISmartFarmerAIControllerServiceProvider aiProvider,
        ISmartFarmerPlantControllerService plantService,
        ISmartFarmerRepository repository)
    {
        _aiProvider = aiProvider;
        _plantService = plantService;
        _repository = repository;
    }

    public async Task<bool> IsValidHoverPlan(string userId, string planId)
    {
        return await GetPlan(userId, planId) != null;
    }

    public async Task<bool> AnalyseHoverPlanResult(
        string userId,
        IFarmerPlanExecutionResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        if (string.IsNullOrEmpty(result.PlanId)) throw new InvalidOperationException($"invalid plan result");

        var plan = await GetPlan(userId, result.PlanId);
        if (plan == null) throw new InvalidOperationException($"invalid plan for {result.PlanId}");

        await AnalysePlanCore(userId, plan, result);
        await RemoveStoredPlan(userId, result.PlanId);

        return true;
    }

    public async Task<IFarmerPlan> GenerateHoverPlan(string userId, string plantInstanceId)
    {
        var plantInstance = await GetPlantInstance(plantInstanceId, userId);
        
        var aiModule = _aiProvider.GetAIPlantPlanGenerator(plantInstance);
        if (aiModule == null)
        {
            SmartFarmerLog.Error($"no such AI Module found for plant {plantInstance}");
            return null;
        }

        var hoverPlan = await aiModule.GenerateHoverPlan(plantInstance) as FarmerPlan;

        if (hoverPlan == null)
        {
            return hoverPlan;
        }

        await StoreHoverPlan(userId, hoverPlan);

        return hoverPlan;
    }

    private async Task<IFarmerPlan> GetPlan(string userId, string planId)
    {
        return await _repository.GetFarmerPlanByIdAsync(planId, userId);
    }

    private async Task<FarmerPlantInstance> GetPlantInstance(string plantInstanceId, string userId)
    {
        return await _plantService.GetFarmerPlantInstanceByIdForUserAsync(userId, plantInstanceId) as FarmerPlantInstance;
    }

    private async Task StoreHoverPlan(string userId, FarmerPlan hoverPlan)
    {
        await _repository.SaveFarmerPlan(hoverPlan);
    }

    private async Task RemoveStoredPlan(string userId, string planId)
    {
        var plan = await GetPlan(userId, planId) as FarmerPlan;

        if (plan != null)
        {
            await _repository.DeleteFarmerPlan(plan);
        }
    }

    private async Task<FarmerAIDetectionLog> AnalysePlanCore(
        string userId, 
        IFarmerPlan plan, 
        IFarmerPlanExecutionResult result)
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

        if (!result.StepResults.Any())
        {
            return log;
        }

        foreach (var stepResult in result.StepResults)
        {
            SmartFarmerLog.Debug($"processing step {stepResult.StepId}");
            FarmerAIDetectionLog detectionLog = null;

            // check if refers to PlantInstance
            if (!string.IsNullOrEmpty(stepResult.PlantInstanceId))
            {
                detectionLog = await AnalysePlantBasedStep(
                    userId, 
                    stepResult.StepId, 
                    stepResult.PlantInstanceId, 
                    stepResult.Result);
            }
            else
            {
                detectionLog = await AnalyseTaskBasedStep(
                    userId, 
                    stepResult.StepId, 
                    stepResult.TaskInterfaceFullName, 
                    stepResult.Result);
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
        
        var aiModule = _aiProvider.GetAIPlantDetector(plantInstance);
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
        var aiModule = _aiProvider.GetAITaskDetector(taskInterfaceFullName);
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
}
