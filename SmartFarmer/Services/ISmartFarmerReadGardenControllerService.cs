using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Services;

public interface ISmartFarmerReadGardenControllerService
{
    event EventHandler<DevicePositionEventArgs> NewDevicePosition;
    event EventHandler<NewPlantEventArgs> NewPlantInGarden;
    event EventHandler<PlanEventArgs> NewPlan;
    event EventHandler<PlanEventArgs> PlanDeleted;
    event EventHandler<PlanEventArgs> NewAutoIrrigationPlan;
    event EventHandler<NewAlertEventArgs> NewAlert;
    event EventHandler<NewAlertStatusEventArgs> NewAlertStatus;

    Task<IEnumerable<IFarmerGarden>> GetFarmerGardenByUserIdAsync(string userId);
    Task<IFarmerGarden> GetFarmerGardenByIdForUserAsync(string userId, string gardenId);

    Task<IFarmerPlantInstance> GetFarmerPlantInstanceByIdForUserAsync(string userId, string plantId);
    Task<IEnumerable<IFarmerPlantInstance>> GetFarmerPlantInstanceByIdsForUserAsync(string userId, string[] plantIds);

    Task<IFarmerPlant> GetFarmerPlantByIdAsync(string plantId);
    Task<IEnumerable<IFarmerPlant>> GetFarmerPlantByIdsAsync(string[] plantIds);
    Task<IrrigationHistory> GetFarmerIrrigationHistoryByPlantAsync(string userId, string plantId);
    Task<bool> MarkIrrigationInstance(string userId, FarmerPlantIrrigationInstance irrigationInstance);
    
    Task<IEnumerable<string>> GetFarmerPlanIdsInGardenAsync(string userId, string gardenId);
    Task<IFarmerPlan> GetFarmerPlanByIdForUserAsync(string userId, string planId);
    Task<IEnumerable<IFarmerPlan>> GetFarmerPlanByIdsForUserAsync(string userId, string[] planIds);
    Task<IEnumerable<IFarmerPlanStep>> GetFarmerPlanStepByIdsAsync (string[] ids);

    Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByGardenIdAsync(string userId, string gardenId);
    Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByIdAsync(string userId, string[] ids);
}
