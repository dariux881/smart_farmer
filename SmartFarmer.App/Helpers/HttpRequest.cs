using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SmartFarmer.Misc;

namespace SmartFarmer.Helpers;

public class HttpRequest
{
    public Exception LastException {get; private set; }

    public async Task<HttpResponseMessage> GetAsync(
        string uri, 
        CancellationToken token,
        KeyValuePair<string, string>[] parameters = null, 
        bool includeAuthentication = true)
    {
        using (var client = new HttpClient())
        {
            PrepareHttpClient(client, includeAuthentication);

            var builder = new UriBuilder(client.BaseAddress + uri);
            builder.Port = client.BaseAddress.Port;

            var query = HttpUtility.ParseQueryString(builder.Query);
            if (parameters != null && parameters.Any())
            {
                foreach (var parameter in parameters)
                {
                    query[parameter.Key] = parameter.Value;
                }
            }
            builder.Query = query.ToString();
            var url = builder.ToString();

            try
            {
                var response = await client.GetAsync(url, token);
                SmartFarmerLog.Debug("Response received for uri=\"" + url + "\"");

                return response;
            }
            catch (HttpRequestException e)
            {
                SmartFarmerLog.Exception(e);
                LastException = e;
                return null;
            }
        }
    }

    public async Task<HttpResponseMessage> PostAsync<T>(
        string uri, 
        T body,
        CancellationToken token, 
        KeyValuePair<string, string>[] parameters = null, 
        bool includeAuthentication = true)
    {
        using (var client = new HttpClient())
        {
            PrepareHttpClient(client, includeAuthentication);

            try
            {
                var response = await client.PostAsJsonAsync(uri, body, token);
                SmartFarmerLog.Debug("Response received for uri=\"" + uri+ "\"");
                return response;
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
                    LocalConfiguration.Token);
        }
    }
}