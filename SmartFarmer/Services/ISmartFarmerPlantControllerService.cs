using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.Plants;

namespace SmartFarmer.Services;

public interface ISmartFarmerPlantControllerService
{
    event EventHandler<NewPlantEventArgs> NewPlantInGarden;

    Task<bool> AddFarmerPlantInstance(string userId, FarmerPlantRequestData data);

    Task<IFarmerPlantInstance> GetFarmerPlantInstanceByIdForUserAsync(string userId, string plantId);
    Task<IEnumerable<IFarmerPlantInstance>> GetFarmerPlantInstanceByIdsForUserAsync(string userId, string[] plantIds);
    Task<IFarmerPlant> GetFarmerPlantByIdAsync(string plantId);
    Task<IEnumerable<IFarmerPlant>> GetFarmerPlantByIdsAsync(string[] plantIds);
    Task<IrrigationHistory> GetFarmerIrrigationHistoryByPlantAsync(string userId, string plantId);
    Task<bool> MarkIrrigationInstance(string userId, FarmerPlantIrrigationInstance irrigationInstance);
}
