using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.DTOs.Alerts;

namespace SmartFarmer.Data;

public interface ISmartFarmerAlertManagementRepository
{
    Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByIdsAsync(string userId, string[] ids);
    Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByGardenIdAsync(string userId, string gardenId);

    Task<string> CreateFarmerAlert(string userId, FarmerAlertRequestData data);
    Task<bool> MarkFarmerAlertAsReadAsync(string userId, string alertId, bool read);
}