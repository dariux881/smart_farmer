
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartFarmer.Data.Tasks;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Handlers;

////
// Partial class for Garden management
////
public partial class FarmerRequestHandler
{
    public static async Task<IFarmerPlan> GetHoverPlanForPlant(string plantId, CancellationToken token)
    {
        var httpReq = new HttpRequest();

        try
        {
            var response = await 
                httpReq
                    .GetAsync(
                        SmartFarmerApiConstants.GENERATE_PLAN_FOR_PLANT,
                        token,
                        new KeyValuePair<string, string>[] { 
                            new KeyValuePair<string, string>(
                                "plantId", 
                                plantId) });
            
            if (response == null || !response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FarmerPlan>((JsonSerializerOptions)null, token);
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);

            return null;
        }
    }
}