using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.Movement;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;

namespace SmartFarmer.Services;

public interface ISmartFarmerGardenControllerService
{
    event EventHandler<DevicePositionEventArgs> NewDevicePosition;

    Task<IEnumerable<IFarmerGarden>> GetFarmerGardenByUserIdAsync(string userId);
    Task<IFarmerGarden> GetFarmerGardenByIdForUserAsync(string userId, string gardenId);

    Task<IFarmerGarden> CreateFarmerGarden(string userId, FarmerGardenRequestData data);

    IFarmerCliCommand BuildAndCheckCliCommand(string userId, string gardenId, string commandStr);
    Task<FarmerDevicePosition> NotifyDevicePosition(string userId, FarmerDevicePositionRequestData position);
    Task<bool> NotifyDevicePositions(string userId, FarmerDevicePositionsRequestData positions);
    Task<IEnumerable<FarmerDevicePosition>> GetDeviceDevicePositionHistory(string userId, string gardenId, string runId);


}