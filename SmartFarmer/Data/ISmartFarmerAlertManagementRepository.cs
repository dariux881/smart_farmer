using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Alerts;

namespace SmartFarmer.Data;

public interface ISmartFarmerAlertManagementRepository
{
    Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByIdsAsync(string userId, string[] ids);
    Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByGroundIdAsync(string userId, string groundId);
    Task MarkFarmerAlertAsReadAsync(string userId, string alertId, bool read);
}