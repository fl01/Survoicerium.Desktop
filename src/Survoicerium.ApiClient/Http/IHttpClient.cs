using System.Threading.Tasks;

namespace Survoicerium.ApiClient.Http
{
    public interface IHttpClient
    {
        Task<HttpResponse<TResponse>> GetAsync<TResponse>(string requestUri)
            where TResponse : class;

        Task<HttpResponse> PostAsync<TEntity>(TEntity entity, string requestUri)
            where TEntity : class;

        void SetCustomHeader(string header, string headerValue);
    }
}
