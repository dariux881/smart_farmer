using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Data.Security;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;

namespace SmartFarmer.Handlers;

////
// Partial class for Authentication management
////
public partial class FarmerRequestHandler
{
    public static async Task<LoginResponseData> Login(LoginRequestData data, CancellationToken token)
    {
         var httpReq = new HttpRequest();

        var response = await 
            httpReq
                .PostAsync(
                    SmartFarmerApiConstants.USER_LOGIN_API,
                    data,
                    token, 
                    null, 
                    false);
        
        if (response != null && response.IsSuccessStatusCode)
        {
            var responseStr = await response.Content.ReadAsStringAsync(token);
            return responseStr.Deserialize<LoginResponseData>();
        }

        return null;
    }

}