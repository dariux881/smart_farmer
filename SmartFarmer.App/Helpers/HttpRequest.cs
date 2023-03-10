using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using SmartFarmer.Misc;

namespace SmartFarmer.Helpers;

public class HttpRequest
{
    public Exception LastException {get; private set; }

    public async Task<string> GetAsync(string uri, List<KeyValuePair<string, string>> parameters = null, bool includeAuthentication = true)
    {
        using (var client = new HttpClient())
        {
            PrepareHttpClient(client, includeAuthentication);

            var query = HttpUtility.ParseQueryString(uri);
            if (parameters != null && parameters.Any())
            {
                foreach (var parameter in parameters)
                {
                    query[parameter.Key] = parameter.Value;
                }
            }
            string queryString = query.ToString();

            try
            {
                string responseBody = await client.GetStringAsync(queryString);
                SmartFarmerLog.Debug("Response received for uri=\"" + queryString + "\"");

                return responseBody;
            }
            catch (HttpRequestException e)
            {
                SmartFarmerLog.Exception(e);
                LastException = e;
                return null;
            }
        }
    }

    
    public async Task<string> PostAsync<T>(string uri, T body, List<KeyValuePair<string, string>> parameters = null, bool includeAuthentication = true)
    {
        using (var client = new HttpClient())
        {
            PrepareHttpClient(client, includeAuthentication);

            try
            {
                var response = await client.PostAsJsonAsync(uri, body);
                SmartFarmerLog.Debug("Response received for uri=\"" + uri+ "\"");

                if (response.IsSuccessStatusCode)
                {
                    // Get the URI of the created resource.
                    var returnContent = await response.Content?.ReadAsStringAsync();
                    return returnContent;
                }

                return null;
            }
            catch (HttpRequestException e)
            {
                SmartFarmerLog.Exception(e);
                LastException = e;
                return null;
            }
        }
    }

    private void PrepareHttpClient(HttpClient client, bool includeAuthentication)
    {
        client.BaseAddress = new Uri( ApiConfiguration.BaseAddress );
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (includeAuthentication)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    SmartFarmerApiConstants.USER_AUTHENTICATION_HEADER_KEY, 
                    ApiConfiguration.Token);
        }
    }
}