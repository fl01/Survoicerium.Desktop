using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Survoicerium.ApiClient.Http;
using Survoicerium.ApiClient.Models;

namespace Survoicerium.ApiClient
{
    public class Api
    {
        private readonly string _baseUrl;
        private readonly IHttpClient _httpClient;
        private string _apiKey = null;

        public Api(string baseUrl, IHttpClient httpClient)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;
        }

        public void GetApiKey(string hardwareId)
        {
            Process.Start(Path.Combine(_baseUrl, "getapikey"));
        }

        public Api UseApiKey(string key)
        {
            _apiKey = key;
            _httpClient.SetCustomHeader("X-ApiKey", key);
            return this;
        }

        public async Task<bool> VerifyApiKeyAsync()
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return false;
            }

            string requestUrl = $"{_baseUrl}/api/me";
            var result = await _httpClient.GetAsync<User>(requestUrl);
            return (result?.IsSuccess).GetValueOrDefault();
        }
    }
}
