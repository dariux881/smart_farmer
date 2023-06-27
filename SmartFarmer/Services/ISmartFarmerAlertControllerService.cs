using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Alerts;

namespace SmartFarmer.Services;

public interface ISmartFarmerAlertControllerService
{
    event EventHandler<NewAlertEventArgs> NewAlert;
    event EventHandler<NewAlertStatusEventArgs> NewAlertStatus;

    Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByGardenIdAsync(string userId, string gardenId);
    Task<IEnumerable<IFarmerAlert>> GetFarmerAlertsByIdAsync(string userId, string[] ids);

    Task<string> CreateFarmerAlert(string userId, FarmerAlertRequestData data);
    Task<bool> MarkFarmerAlertAsRead(string userId, string id, bool read);
}
