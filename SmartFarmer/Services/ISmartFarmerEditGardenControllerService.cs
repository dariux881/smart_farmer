using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.Movement;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;

namespace SmartFarmer.Services;

public interface ISmartFarmerEditGardenControllerService
{
    Task<string> CreateFarmerAlert(string userId, FarmerAlertRequestData data);
    Task<bool> MarkFarmerAlertAsRead(string userId, string id, bool read);

    Task<IFarmerGarden> CreateFarmerGarden(string userId, FarmerGardenRequestData data);
    Task<bool> AddFarmerPlantInstance(string userId, FarmerPlantRequestData data);
    Task<string> AddPlan(string userId, FarmerPlanRequestData planRequestData);
    Task<bool> DeletePlan(string userId, string planId);
    Task<string> BuildIrrigationPlan(string userId, string gardenId);

    Task<FarmerDevicePosition> NotifyDevicePosition(string userId, FarmerDevicePositionRequestData position);
    Task<bool> NotifyDevicePositions(string userId, FarmerDevicePositionsRequestData positions);

    IFarmerCliCommand BuildAndCheckCliCommand(string userId, string gardenId, string commandStr);

    Task<IEnumerable<FarmerDevicePosition>> GetDeviceDevicePositionHistory(string userId, string gardenId, string runId);
}