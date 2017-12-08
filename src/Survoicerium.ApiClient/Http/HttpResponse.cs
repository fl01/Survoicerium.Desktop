using System.Net;

namespace Survoicerium.ApiClient.Http
{
    public class HttpResponse<T> : HttpResponse
    {
        public T Result { get; set; }
    }

    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public bool IsSuccess { get; set; }
    }
}
