using Survoicerium.ApiClient.Http;
using System.Diagnostics;
using System.IO;

namespace Survoicerium.ApiClient
{
    public class Api
    {
        private readonly string _baseUrl;
        private readonly IHttpClient _httpClient;
        private string _apiKey = null;

        public Api(string baseUrl, IHttpClient httpClient = null)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;
        }

        /// <summary>
        /// TODO: should we return existing apikey if user with same hardwareid exists on a server ?
        /// </summary>
        /// <param name="hardwareId"></param>
        public void GetApiKey(string hardwareId)
        {
            Process.Start(Path.Combine(_baseUrl, "getapikey"));
        }

        public Api UseApiKey(string key)
        {
            _apiKey = key;
            return this;
        }
    }
}
