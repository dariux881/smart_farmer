using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.DTOs;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Misc;
using SmartFarmer.Services.AI;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Tasks.Movement;

namespace SmartFarmer.Services.Plan;

public class SmartFarmerPlanControllerService : ISmartFarmerPlanControllerService
{
    private readonly ISmartFarmerRepository _repository;
    private readonly ISmartFarmerGardenControllerService _gardenController;
    private readonly ISmartFarmerAIControllerService _aiService;

    public SmartFarmerPlanControllerService(
        ISmartFarmerRepository repository,
        ISmartFarmerGardenControllerService gardenController,
        ISmartFarmerAIControllerService aiService)
    {
        _repository = repository;

        _gardenController = gardenController;
        _aiService = aiService;
    }

    public event EventHandler<PlanEventArgs> NewPlan;
    public event EventHandler<PlanEventArgs> PlanDeleted;
    public event EventHandler<PlanEventArgs> NewAutoIrrigationPlan;

    public async Task<IFarmerPlan> GetFarmerPlanByIdForUserAsync(string userId, string planId)
    {
        return await _repository.GetFarmerPlanByIdAsync(planId, userId);
    }
    
    public async Task<IEnumerable<string>> GetFarmerPlanIdsInGardenAsync(string userId, string gardenId)
    {
        return await _repository.GetFarmerPlansInGarden(gardenId, userId);
    }

    public async Task<IEnumerable<IFarmerPlan>> GetFarmerPlanByIdsForUserAsync(string userId, string[] planIds)
    {
        return await _repository.GetFarmerPlanByIdsAsync(planIds, userId);
    }

    public async Task<IEnumerable<IFarmerPlanStep>> GetFarmerPlanStepByIdsAsync (string[] ids)
    {
        return await _repository.GetFarmerPlanStepByIdsAsync(ids);
    }

    public async Task<string> AddPlan(string userId, FarmerPlanRequestData planRequestData)
    {
        if (planRequestData == null) throw new ArgumentNullException(nameof(planRequestData));
        if (string.IsNullOrEmpty(planRequestData.GardenId)) throw new ArgumentNullException(nameof(planRequestData.GardenId));
        if (planRequestData.Steps.IsNullOrEmpty()) throw new ArgumentNullException(nameof(planRequestData.Steps));

        var garden = await _gardenController.GetFarmerGardenByIdForUserAsync(userId, planRequestData.GardenId) as FarmerGarden;
        if (garden == null) return null; // no valid garden

        var plan = 
            new FarmerPlan()
            {
                Name = planRequestData.PlanName,
                GardenId = planRequestData.GardenId,
                CronSchedule = planRequestData.CronSchedule,
                Priority = planRequestData.Priority,
                ValidFromDt = planRequestData.ValidFromDt,
                ValidToDt = planRequestData.ValidToDt
            };

        foreach (var reqStep in planRequestData.Steps)
        {
            var step = 
                new FarmerPlanStep()
                {
                    BuildParameters = reqStep.BuildParameters,
                    Delay = reqStep.Delay,
                    TaskClassFullName = reqStep.TaskClassFullName,
                    TaskInterfaceFullName = reqStep.TaskInterfaceFullName
                };
            
            plan.Steps.Add(step);
        }

        var planId = await _repository.SaveFarmerPlan(plan);

        if (!string.IsNullOrEmpty(planId))
        {
            NewPlan?.Invoke(this, new PlanEventArgs(plan.GardenId, planId));
        }

        return planId;
    }

    public async Task<bool> DeletePlan(string userId, string planId)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(planId)) throw new ArgumentNullException(nameof(planId));

        var plan = await _repository.GetFarmerPlanByIdAsync(planId, userId) as FarmerPlan;

        var result = await _repository.DeleteFarmerPlan(plan);

        PlanDeleted?.Invoke(this, new PlanEventArgs(plan.GardenId, planId));

        return result;
    }

    public async Task<string> BuildIrrigationPlan(string userId, string gardenId)
    {
        var garden = await _gardenController.GetFarmerGardenByIdForUserAsync(userId, gardenId) as FarmerGarden;
        if (garden == null) return null; // no valid garden

        var plan = CreateIrrigationPlan(garden);

        if (plan == null)
        {
            SmartFarmerLog.Error($"No valid irrigation plan created for garden {garden.ID}");
            return null;
        }

        // return plan ID
        var planId = await _repository.SaveFarmerPlan(plan);

        // save plan to irrigationPlan on garden
        garden.IrrigationPlanId = planId;

        var settings = await _repository.GetUserSettings(userId);
        if (settings != null)
        {
            plan.CronSchedule = settings.AUTOIRRIGATION_PLANNED_CRONSCHEDULE;
            garden.CanIrrigationPlanStart = settings.AUTOIRRIGATION_AUTOSTART;
        }

        if (!await _repository.SaveGardenUpdates())
        {
            SmartFarmerLog.Error($"Failing in saving {planId} as irrigation plan for garden {garden.ID}");
            return null;
        }

        NewAutoIrrigationPlan?.Invoke(this, new PlanEventArgs(gardenId, planId));

        return planId;
    }

    public async Task AnalysePlanResult(string userId, IFarmerPlanExecutionResult result)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
        if (result == null) throw new ArgumentNullException(nameof(result));

        var plan = await GetFarmerPlanByIdForUserAsync(userId, result.PlanId) as FarmerPlan;

        if (_aiService.IsValidHoverPlan(userId, result.PlanId))
        {
            await _aiService.AnalyseHoverPlanResult(userId, plan, result);
            return;
        }
    }

    private FarmerPlan CreateIrrigationPlan(FarmerGarden garden)
    {
        // Steps:
        // - list all plants, minimizing movements
        var orderedPlants = 
            OrderPlantsToMinimizeMovements(garden.Plants);
        
        var steps = CreateSteps(orderedPlants);

        return new FarmerPlan()
        {
            GardenId = garden.ID,
            Name = $"AutoIrrigationPlan_garden{garden.ID}_{DateTime.UtcNow}",
            Steps = steps
        };
    }

    private List<FarmerPlanStep> CreateSteps(List<FarmerPlantInstance> orderedPlants)
    {
        var steps = new List<FarmerPlanStep>();

        orderedPlants.ForEach(plant => {
            var singlePlantSteps = new List<FarmerPlanStep>() {
                // move to plant
                new FarmerPlanStep()
                {
                    BuildParameters = 
                        new Dictionary<string, string>() 
                        {
                            { nameof(IHasTargetGridPosition.TargetXInCm), ""+plant.PlantX },
                            { nameof(IHasTargetGridPosition.TargetYInCm), ""+plant.PlantY }
                        },
                    TaskInterfaceFullName = typeof(IFarmerMoveOnGridTask).FullName
                },
                // check water
                new FarmerPlanStep()
                {
                    BuildParameters = 
                        new Dictionary<string, string>() 
                        {
                            { nameof(IFarmerCheckIfWaterIsNeededTask.ExpectedAmountInLiters), ""+plant.Plant.IrrigationTaskInfo.AmountOfWaterInLitersPerTime }
                        },
                    TaskInterfaceFullName = typeof(IFarmerCheckIfWaterIsNeededTask).FullName
                },
                // provide water, if needed
                new FarmerPlanStep()
                {
                    BuildParameters = 
                        new Dictionary<string, string>() 
                        {
                            { nameof(IFarmerProvideWaterTask.WaterAmountInLiters), ""+plant.Plant.IrrigationTaskInfo.AmountOfWaterInLitersPerTime }
                        },
                    TaskInterfaceFullName = typeof(IFarmerProvideWaterTask).FullName
                }
            };

            steps.AddRange(singlePlantSteps);
        });

        return steps;
    }

    private List<FarmerPlantInstance> OrderPlantsToMinimizeMovements(List<FarmerPlantInstance> plants)
    {
        return 
            plants
                .OrderBy(p => p.PlantX)
                .ThenBy(p => p.PlantY)
                .ToList();
    }

}
