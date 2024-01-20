using TotovBuilder.AzureFunctions.Wrappers;

namespace TotovBuilder.AzureFunctions.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionalities of a <see cref="HttpClientWrapper"/> wrapper.
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
