using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SmartFarmer.Misc;

namespace SmartFarmer.Helpers;

public class HttpRequest
{
    public Exception LastException {get; private set; }

    public async Task<string> GetAsync(string uri)
    {
        using (var client = new HttpClient())
        {
            PrepareHttpClient(client);

            try
            {
                string responseBody = await client.GetStringAsync(uri);
                SmartFarmerLog.Debug("Response received for uri=\"" + uri+ "\"");

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

    
    public async Task<string> PostAsync<T>(string uri, T parameter)
    {
        using (var client = new HttpClient())
        {
            PrepareHttpClient(client);

            try
            {
                var response = await client.PostAsJsonAsync(uri, parameter);
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

    private void PrepareHttpClient(HttpClient client)
    {
        client.BaseAddress = new Uri( ApiConfiguration.BaseAddress );
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}