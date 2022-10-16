using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstractions;

namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents an HTTP client wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HttpClientWrapper : IHttpClientWrapper
    {
        /// <summary>
        /// Instance.
        /// </summary>
        private HttpClient Instance { get; }

        /// <summary>
        /// Initializes a new instance of a <see cref="HttpClientWrapper"/> class.
        /// </summary>
        /// <param name="httpClientFactory">HTTP client factory.</param>
        public HttpClientWrapper(IHttpClientFactory httpClientFactory)
        {
            Instance = httpClientFactory.CreateClient();
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return Instance.SendAsync(request);
        }
    }
}
