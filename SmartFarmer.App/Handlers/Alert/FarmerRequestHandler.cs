using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Data.Alerts;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;

namespace SmartFarmer.Handlers;

////
// Partial class for Alert management
////
public partial class FarmerRequestHandler
{
    public static async Task<IFarmerAlert> GetAlert(string alertId, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        var response = await 
            httpReq
                .GetAsync(
                    SmartFarmerApiConstants.GET_ALERTS,
                    token,
                    new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("ids", alertId) });
        
        if (response == null || !response.IsSuccessStatusCode)
        {
            SmartFarmerLog.Warning(response.ReasonPhrase);
            return null;
        }

        var alertStr = await response.Content.ReadAsStringAsync(token);
        var alerts = alertStr.Deserialize<List<FarmerAlert>>();

        if (alerts != null && alerts.Any())
        {
            return alerts.First();
        }

        return null;
    }

    public static async Task<IEnumerable<IFarmerAlert>> GetAlerts(string[] ids, CancellationToken token)
    {
        if (ids == null) throw new ArgumentNullException(nameof(ids));
        if (!ids.Any())
        {
            await Task.CompletedTask;
            return new List<IFarmerAlert>();
        }

        var httpReq = new HttpRequest();

        var response = await 
            httpReq
                .GetAsync(
                    SmartFarmerApiConstants.GET_ALERTS,
                    token,
                    new KeyValuePair<string, string>[] 
                    { 
                        new KeyValuePair<string, string>(
                            "ids", 
                            ids.Aggregate((p1, p2) => p1 + "#" + p2)) });
        
        if (response == null || !response.IsSuccessStatusCode)
        {
            SmartFarmerLog.Warning(response.ReasonPhrase);
            return null;
        }

        var alertStr = await response.Content.ReadAsStringAsync(token);
        return alertStr.Deserialize<List<FarmerAlert>>() as IEnumerable<IFarmerAlert>;
    }

    public static async Task<string> RaiseAlert(FarmerAlertRequestData data, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        var response = 
            await httpReq
                .PostAsync(
                    SmartFarmerApiConstants.CREATE_ALERT,
                    data, 
                    token);

        if (response == null || !response.IsSuccessStatusCode)
        {
            SmartFarmerLog.Warning(response.ReasonPhrase);
            return null;
        }

        var returnContent = (await response.Content?.ReadAsStringAsync(token)).RemoveAdditionalQuotes();

        return returnContent;
    }

    public static async Task<bool> MarkAlertAsRead(string alertId, bool read, CancellationToken token)
    {
        // markAlert
        var httpReq = new HttpRequest();

        var response = await 
            httpReq
                .GetAsync(
                    SmartFarmerApiConstants.SET_ALERT_READ,
                    token,
                    new KeyValuePair<string, string>[] 
                    { 
                        new KeyValuePair<string, string>("alertId", alertId),
                        new KeyValuePair<string, string>("read", "" + read),
                    });
        
        if (response == null || !response.IsSuccessStatusCode)
        {
            SmartFarmerLog.Warning(response.ReasonPhrase);
            return false;
        }

        var resultStr = await response.Content.ReadAsStringAsync();
        return bool.Parse(resultStr);
    }
}
