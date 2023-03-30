using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Alerts;
using SmartFarmer.Data.Alerts;
using SmartFarmer.Misc;

namespace SmartFarmer.Helpers;

////
// Partial class for Alert management
////
public partial class FarmerRequestHelper
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
            return null;
        }

        var alertStr = await response.Content.ReadAsStringAsync(token);
        return alertStr.Deserialize<List<FarmerAlert>>() as IEnumerable<IFarmerAlert>;
    }

    public static async Task<string> RaiseAlert(FarmerAlertRequestData data, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        var result = 
            await httpReq
                .PostAsync(
                    SmartFarmerApiConstants.CREATE_ALERT,
                    data, 
                    token);

        if (result == null || !result.IsSuccessStatusCode)
        {
            return null;
        }

        var returnContent = await result.Content?.ReadAsStringAsync();
        return returnContent;
    }
}
