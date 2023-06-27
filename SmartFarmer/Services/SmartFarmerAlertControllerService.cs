using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Data;
using SmartFarmer.DTOs.Alerts;

namespace SmartFarmer.Services;

public class SmartFarmerAlertControllerService : ISmartFarmerAlertControllerService 
{
    private readonly ISmartFarmerRepository _repository;

    public SmartFarmerAlertControllerService(ISmartFarmerRepository repository)
    {
        _repository = repository;
    }

    public event EventHandler<NewAlertEventArgs> NewAlert;
    public event EventHandler<NewAlertStatusEventArgs> NewAlertStatus;


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

}
