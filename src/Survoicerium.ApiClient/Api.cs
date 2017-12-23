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
        private readonly string _frontend;
        private readonly IHttpClient _httpClient;
        private string _apiKey = null;

        public Api(string backend, string frontend, IHttpClient httpClient)
        {
            _baseUrl = backend;
            _frontend = frontend;
            _httpClient = httpClient;
        }

        public void GetApiKey(string hardwareId)
        {
            Process.Start(Path.Combine(_frontend, "getapikey?state=new"));
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

        public async Task RequestToJoinVoiceChannelAsync(string hash)
        {
            await _httpClient.PostAsync<GameInfo>(new GameInfo() { Hash = hash }, $"{_baseUrl}/api/game");
        }
    }
}
