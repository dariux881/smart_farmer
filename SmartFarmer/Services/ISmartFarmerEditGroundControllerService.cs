using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.Movement;
using SmartFarmer.Plants;

namespace SmartFarmer.Services;

public interface ISmartFarmerEditGroundControllerService
{
    Task<string> CreateFarmerAlert(string userId, FarmerAlertRequestData data);
    Task<bool> MarkFarmerAlertAsRead(string userId, string id, bool read);

    Task<IFarmerGround> CreateFarmerGround(string userId, FarmerGroundRequestData data);
    Task<bool> AddFarmerPlantInstance(string userId, FarmerPlantRequestData data);
    Task<string> BuildIrrigationPlan(string userId, string groundId);

    Task<bool> NotifyDevicePosition(string userId, FarmerDevicePositionRequestData position);
    Task<IEnumerable<FarmerDevicePosition>> GetDeviceDevicePositionHistory(string userId, string groundId, string runId);
}
