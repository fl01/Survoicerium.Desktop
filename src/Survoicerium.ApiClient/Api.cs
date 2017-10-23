using System.Diagnostics;
using System.IO;

namespace Survoicerium.ApiClient
{
    public class Api
    {
        private readonly string _baseUrl;

        public Api(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// TODO: should we return existing apikey if user with same hardwareid exists on a server ?
        /// </summary>
        /// <param name="hardwareId"></param>
        public void GetApiKey(string hardwareId)
        {
            Process.Start(Path.Combine(_baseUrl, "getapikey"));
        }
    }
}
