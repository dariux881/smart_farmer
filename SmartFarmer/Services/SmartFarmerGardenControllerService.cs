using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Data;
using SmartFarmer.DTOs;
using SmartFarmer.DTOs.Alerts;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Tasks.Movement;

namespace SmartFarmer.Services;

public class SmartFarmerGardenControllerService : ISmartFarmerGardenControllerService 
{
    private readonly ISmartFarmerRepository _repository;

    public SmartFarmerGardenControllerService(ISmartFarmerRepository repository)
    {
        _repository = repository;
    }

    public event EventHandler<DevicePositionEventArgs> NewDevicePosition;
    public event EventHandler<DevicePositionsEventArgs> NewDevicePositions;
    public event EventHandler<NewPlantEventArgs> NewPlantInGarden;
    public event EventHandler<PlanEventArgs> NewPlan;
    public event EventHandler<PlanEventArgs> PlanDeleted;
    public event EventHandler<PlanEventArgs> NewAutoIrrigationPlan;
    public event EventHandler<NewAlertEventArgs> NewAlert;
    public event EventHandler<NewAlertStatusEventArgs> NewAlertStatus;

    public async Task<IEnumerable<IFarmerGarden>> GetFarmerGardenByUserIdAsync(string userId)
    {
        return await _repository.GetFarmerGardenByUserIdAsync(userId);
    }
    
    public async Task<IFarmerGarden> GetFarmerGardenByIdForUserAsync(string userId, string gardenId)
    {
        return await _repository.GetFarmerGardenByIdForUserAsync(userId, gardenId);
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

    public async Task<IrrigationHistory> GetFarmerIrrigationHistoryByPlantAsync(string userId, string plantId)
    {
        return await _repository.GetFarmerIrrigationHistoryByPlant(plantId, userId);
    }
    
    public async Task<bool> MarkIrrigationInstance(string userId, FarmerPlantIrrigationInstance irrigationInstance)
    {
        return await _repository.MarkIrrigationInstance(irrigationInstance, userId);
    }

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

    public async Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByGardenIdAsync(string userId, string gardenId)
    {
        return await _repository.GetFarmerAlertsByGardenIdAsync(userId, gardenId);
    }

    public async Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByIdAsync(string userId, string[] ids)
    {
        return await _repository.GetFarmerAlertsByIdsAsync(userId, ids);
    }

    public async Task<string> CreateFarmerAlert(string userId, FarmerAlertRequestData data)
    {
        var alertId = await _repository.CreateFarmerAlert(userId, data);

        NewAlert?.Invoke(this, new NewAlertEventArgs(data.GardenId, alertId));

        return alertId;
    }

    public async Task<bool> MarkFarmerAlertAsRead(string userId, string id, bool read)
    {
        var alertStatusChanged = await _repository.MarkFarmerAlertAsReadAsync(userId, id, read);

        if (alertStatusChanged)
        {
            var alert = (await this.GetFarmerAlertsByIdAsync(userId, new [] { id })).FirstOrDefault() as FarmerAlert;

            if (alert != null)
            {
                NewAlertStatus?.Invoke(
                    this, 
                    new NewAlertStatusEventArgs(
                        alert.FarmerGardenId, 
                        id, 
                        alert.MarkedAsRead));
            }
        }

        return alertStatusChanged;
    }

    public async Task<IFarmerGarden> CreateFarmerGarden(string userId, FarmerGardenRequestData data)
    {
        return await _repository.CreateFarmerGarden(userId, data);
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

    public async Task<string> AddPlan(string userId, FarmerPlanRequestData planRequestData)
    {
        if (planRequestData == null) throw new ArgumentNullException(nameof(planRequestData));
        if (string.IsNullOrEmpty(planRequestData.GardenId)) throw new ArgumentNullException(nameof(planRequestData.GardenId));
        if (planRequestData.Steps.IsNullOrEmpty()) throw new ArgumentNullException(nameof(planRequestData.Steps));

        var garden = await GetFarmerGardenByIdForUserAsync(userId, planRequestData.GardenId) as FarmerGarden;
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
        var garden = await GetFarmerGardenByIdForUserAsync(userId, gardenId) as FarmerGarden;
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

    public IFarmerCliCommand BuildAndCheckCliCommand(string userId, string gardenId, string commandStr)
    {
        IFarmerCliCommand command = BuildCliCommand(userId, gardenId, commandStr);

        // check command
        if (!IsCliCommandValid(command))
        {
            return null;
        }

        return command;
    }

    public async Task<FarmerDevicePosition> NotifyDevicePosition(string userId, FarmerDevicePositionRequestData position)
    {
        if (position == null) throw new ArgumentNullException(nameof(position));

        var garden = await GetFarmerGardenByIdForUserAsync(userId, position.GardenId) as FarmerGarden;
        if (garden == null) return null; // no valid garden

        var storedPosition = await _repository.SaveDevicePosition(userId, position);

        if (storedPosition != null)
        {
            NewDevicePosition?.Invoke(this, new DevicePositionEventArgs(storedPosition));
        }

        return storedPosition;
    }

    public async Task<bool> NotifyDevicePositions(string userId, FarmerDevicePositionsRequestData positions)
    {
        if (positions == null) throw new ArgumentNullException(nameof(positions));

        var garden = await GetFarmerGardenByIdForUserAsync(userId, positions.GardenId) as FarmerGarden;
        if (garden == null) return false; // no valid garden

        var ids = await _repository.SaveDevicePositions(userId, positions);

        if (ids != null)
        {
            NewDevicePositions?.Invoke(this, new DevicePositionsEventArgs(ids));
        }

        return ids != null && ids.Any();
    }

    public async Task<IEnumerable<FarmerDevicePosition>> GetDeviceDevicePositionHistory(string userId, string gardenId, string runId)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (gardenId == null) throw new ArgumentNullException(nameof(gardenId));

        var garden = await GetFarmerGardenByIdForUserAsync(userId, gardenId) as FarmerGarden;
        if (garden == null) return null; // no valid garden

        return await _repository.GetDevicePositionHistory(userId, gardenId, runId);
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
                            { nameof(IFarmerMoveOnGridTask.TargetXInCm), ""+plant.PlantX },
                            { nameof(IFarmerMoveOnGridTask.TargetYInCm), ""+plant.PlantY }
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

    private static IFarmerCliCommand BuildCliCommand(string userId, string gardenId, string commandStr)
    {
        if (!ExtractCliCommandParts(commandStr, out var command, out var args))
        {
            return null;
        }

        IFarmerCliCommand cliCommand = new FarmerCliCommand()
        {
            UserId = userId,
            GardenId = gardenId,
            Command = command,
            Args = args
        };
        return cliCommand;
    }

    private static bool ExtractCliCommandParts(string commandStr, out string command, out FarmerCliCommandArgs args)
    {
        command = null;
        args = null;

        if (string.IsNullOrEmpty(commandStr)) return false;

        var commandParts = commandStr.Split(" ");
        command = commandParts[0].Trim();

        if (commandParts.Length == 1)
        {
            args = null;
            return true;
        }

        args = new FarmerCliCommandArgs();
        var commandIndex = 1;

        List<string> referencePartDetails = null;
        while (commandIndex < commandParts.Length)
        {
            var part = commandParts[commandIndex].Trim();
            
            if (part.StartsWith("-"))
            {
                referencePartDetails = new List<string>();
                args.Add(new KeyValuePair<string, List<string>>(part, referencePartDetails));
            }
            else if (referencePartDetails != null)
            {
                referencePartDetails.Add(part);
            }
            else
            {
                // invalid pattern found
                command = null;
                args = null;

                return false;
            }

            commandIndex++;
        }

        return true;
    }

    private bool IsCliCommandValid(IFarmerCliCommand command)
    {
        return true;
    }
}