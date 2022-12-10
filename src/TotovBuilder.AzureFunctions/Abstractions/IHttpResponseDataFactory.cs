using Microsoft.Azure.Functions.Worker.Http;

namespace TotovBuilder.AzureFunctions.Abstractions
{
    /// <summary>
    /// Provides the functionalities of an HTTP data factory.
    /// </summary>
    public interface IHttpResponseDataFactory
    {
        /// <summary>
        /// Creates HTTP response data from HTTP request data.
        /// </summary>
        /// <param name="httpRequestData"></param>
        /// <param name="data">Data to write in the response.</param>
        /// <returns></returns>
        Task<HttpResponseData> CreateResponse(HttpRequestData httpRequestData, object data);
    }
}
