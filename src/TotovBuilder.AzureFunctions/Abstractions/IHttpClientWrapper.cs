using System.Net.Http;
using System.Threading.Tasks;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of an HTTP client wrapper.
    /// </summary>
    public interface IHttpClientWrapper
    {
        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
