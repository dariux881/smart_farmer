using System.Collections.Generic;
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
                    new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("id", alertId) });
        
        if (response == null || !response.IsSuccessStatusCode)
        {
            return null;
        }

        var alertStr = await response.Content.ReadAsStringAsync(token);
        return alertStr.Deserialize<FarmerAlert>();
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
