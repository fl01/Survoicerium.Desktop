using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Survoicerium.ApiClient.Http
{
    public class RetriableHttpClient : IHttpClient
    {
        private readonly TimeSpan _delayBetweenReties;
        private readonly int _maxRetries;
        private Dictionary<string, string> _customHeaders = new Dictionary<string, string>();

        public RetriableHttpClient(TimeSpan delayBetweenReties, int maxRetries)
        {
            _delayBetweenReties = delayBetweenReties;
            _maxRetries = maxRetries;
        }

        public async Task<HttpResponse<TResponse>> GetAsync<TResponse>(string requestUri)
            where TResponse : class
        {
            return await TryRequest(async () => await InternalGetAsync<TResponse>(requestUri));
        }

        public async Task<HttpResponse> PostAsync<TEntity>(TEntity entity, string requestUri)
            where TEntity : class
        {
            return await TryRequest(async () => await InternalPostAsync<TEntity>(entity, requestUri));
        }

        public void SetCustomHeader(string header, string headerValue)
        {
            _customHeaders[header] = headerValue;
        }

        private async Task<HttpResponse> InternalPostAsync<TEntity>(TEntity entity, string requestUri)
            where TEntity : class
        {
            using (var client = CreateClient())
            {
                var json = JsonConvert.SerializeObject(entity);
                var response = await client.PostAsync(requestUri, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

                return new HttpResponse()
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode
                };
            }
        }

        private async Task<HttpResponse<TResponse>> InternalGetAsync<TResponse>(string requestUri)
            where TResponse : class
        {
            using (var client = CreateClient())
            {
                var response = await client.GetAsync(requestUri);

                var responseBody = await response.Content.ReadAsStringAsync();
                return new HttpResponse<TResponse>()
                {
                    Result = JsonConvert.DeserializeObject<TResponse>(responseBody),
                    StatusCode = response.StatusCode,
                    IsSuccess = response.IsSuccessStatusCode
                };
            }
        }

        private async Task<T> TryRequest<T>(Func<Task<T>> action)
        {
            int retry = 0;
            while (retry++ < _maxRetries)
            {
                try
                {
                    return await action();
                }
                catch (HttpRequestException ex)
                {
                    //retriable
                }
                catch (Exception)
                {
                    //non-retriable
                    throw;
                }

                Thread.Sleep(_delayBetweenReties);
            }

            return default(T);
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            foreach (var kvp in _customHeaders)
            {
                client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
            }

            return client;
        }
    }
}
