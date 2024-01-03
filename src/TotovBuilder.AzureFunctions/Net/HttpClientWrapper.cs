using System.Diagnostics.CodeAnalysis;
using TotovBuilder.AzureFunctions.Abstractions.Net;

namespace TotovBuilder.AzureFunctions.Net
{
    /// <summary>
    /// Represents an HTTP client wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the HttpClient class.")]
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
