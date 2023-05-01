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
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Tasks.Movement;

namespace SmartFarmer.Services;

public class SmartFarmerGroundControllerService : ISmartFarmerGroundControllerService 
{
    private readonly ISmartFarmerRepository _repository;

    public SmartFarmerGroundControllerService(ISmartFarmerRepository repository)
    {
        _repository = repository;
    }

    public event EventHandler<DevicePositionEventArgs> NewDevicePosition;
    public event EventHandler<NewPlantEventArgs> NewPlantInGround;
    public event EventHandler<NewPlanEventArgs> NewPlan;
    public event EventHandler<NewPlanEventArgs> NewAutoIrrigationPlan;
    public event EventHandler<NewAlertEventArgs> NewAlert;
    public event EventHandler<NewAlertStatusEventArgs> NewAlertStatus;

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

    public async Task<string> CreateFarmerAlert(string userId, FarmerAlertRequestData data)
    {
        var alertId = await _repository.CreateFarmerAlert(userId, data);

        NewAlert?.Invoke(this, new NewAlertEventArgs(data.FarmerGroundId, alertId));

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
                        alert.FarmerGroundId, 
                        id, 
                        alert.MarkedAsRead));
            }
        }

        return alertStatusChanged;
    }

    public async Task<IFarmerGround> CreateFarmerGround(string userId, FarmerGroundRequestData data)
    {
        return await _repository.CreateFarmerGround(userId, data);
    }

    public async Task<bool> AddFarmerPlantInstance(string userId, FarmerPlantRequestData data)
    {
        var newPlantId = await _repository.AddFarmerPlantInstance(userId, data);

        if (!string.IsNullOrEmpty(newPlantId))
        {
            NewPlantInGround?.Invoke(this, new NewPlantEventArgs(data.FarmerGroundId, newPlantId));
        }

        return newPlantId != null;
    }

    public async Task<string> AddPlan(string userId, FarmerPlan plan, FarmerPlanStep[] steps)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        if (steps == null) throw new ArgumentNullException(nameof(steps));

        var ground = await GetFarmerGroundByIdForUserAsync(userId, plan.GroundId) as FarmerGround;
        if (ground == null) return null; // no valid ground

        plan.Steps.Clear();
        plan.Steps.AddRange(steps);

        var planId = await _repository.SaveFarmerPlan(plan);

        if (!string.IsNullOrEmpty(planId))
        {
            NewPlan?.Invoke(this, new NewPlanEventArgs(plan.GroundId, planId));
        }

        return planId;
    }

    public async Task<string> BuildIrrigationPlan(string userId, string groundId)
    {
        var ground = await GetFarmerGroundByIdForUserAsync(userId, groundId) as FarmerGround;
        if (ground == null) return null; // no valid ground

        var plan = CreateIrrigationPlan(ground);

        if (plan == null)
        {
            SmartFarmerLog.Error($"No valid irrigation plan created for ground {ground.ID}");
            return null;
        }

        // return plan ID
        var planId = await _repository.SaveFarmerPlan(plan);

        // save plan to irrigationPlan on ground
        ground.GroundIrrigationPlanId = planId;

        var settings = await _repository.GetUserSettings(userId);
        if (settings != null)
        {
            plan.CronSchedule = settings.AUTOIRRIGATION_PLANNED_CRONSCHEDULE;
            ground.CanIrrigationPlanStart = settings.AUTOIRRIGATION_AUTOSTART;
        }

        if (!await _repository.SaveGroundUpdates())
        {
            SmartFarmerLog.Error($"Failing in saving {planId} as irrigation plan for ground {ground.ID}");
            return null;
        }

        NewAutoIrrigationPlan?.Invoke(this, new NewPlanEventArgs(groundId, planId));

        return planId;
    }

    public async Task<bool> NotifyDevicePosition(string userId, FarmerDevicePositionRequestData position)
    {
        if (position == null) throw new ArgumentNullException(nameof(position));

        var ground = await GetFarmerGroundByIdForUserAsync(userId, position.GroundId) as FarmerGround;
        if (ground == null) return false; // no valid ground

        var storedPosition = await _repository.SaveDevicePosition(userId, position);

        if (storedPosition != null)
        {
            NewDevicePosition?.Invoke(this, new DevicePositionEventArgs(storedPosition));
        }

        return storedPosition != null;
    }

    public async Task<IEnumerable<FarmerDevicePosition>> GetDeviceDevicePositionHistory(string userId, string groundId, string runId)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (groundId == null) throw new ArgumentNullException(nameof(groundId));

        var ground = await GetFarmerGroundByIdForUserAsync(userId, groundId) as FarmerGround;
        if (ground == null) return null; // no valid ground

        return await _repository.GetDevicePositionHistory(userId, groundId, runId);
    }

    private FarmerPlan CreateIrrigationPlan(FarmerGround ground)
    {
        // Steps:
        // - list all plants, minimizing movements
        var orderedPlants = 
            OrderPlantsToMinimizeMovements(ground.Plants);
        
        var steps = CreateSteps(orderedPlants);

        return new FarmerPlan()
        {
            GroundId = ground.ID,
            Name = $"AutoIrrigationPlan_ground{ground.ID}_{DateTime.UtcNow}",
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
                    BuildParameters = new object[] { plant.PlantX, plant.PlantY },
                    TaskInterfaceFullName = typeof(IFarmerMoveOnGridTask).FullName
                },
                // check water
                new FarmerPlanStep()
                {
                    BuildParameters = new object[] { plant.ID },
                    TaskInterfaceFullName = typeof(IFarmerCheckIfWaterIsNeededTask).FullName
                },
                // provide water, if needed
                new FarmerPlanStep()
                {
                    BuildParameters = new object[] { plant.ID },
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