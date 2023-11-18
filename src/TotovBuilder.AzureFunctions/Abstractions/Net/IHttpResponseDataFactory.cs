using Microsoft.Azure.Functions.Worker.Http;

namespace TotovBuilder.AzureFunctions.Abstractions.Net
{
    /// <summary>
    /// Provides the functionalities of an HTTP data factory.
    /// </summary>
    public interface IHttpResponseDataFactory
    {
        /// <summary>
        /// Creates HTTP response data from HTTP request data.
        /// This method must be used instead of <see cref="CreateResponse(HttpRequestData, object)"/>
        /// when serializing a list of objects derived from a common class in order for all properties to be serialized.
        /// </summary>
        /// <param name="httpRequestData">HTTP request data.</param>
        /// <param name="data">Data to write in the response.</param>
        /// <returns>HTTP response.</returns>
        Task<HttpResponseData> CreateEnumerableResponse(HttpRequestData httpRequestData, IEnumerable<object> data);

        /// <summary>
        /// Creates HTTP response data from HTTP request data.
        /// This method must not be called when serializing a list of objects derived from a common class because in this case,
        /// only common properties are serialized.
        /// Use <see cref="CreateEnumerableResponse(HttpRequestData, IEnumerable{object})"/> instead.
        /// </summary>
        /// <param name="httpRequestData">HTTP request data.</param>
        /// <param name="data">Data to write in the response.</param>
        /// <returns>HTTP response.</returns>
        Task<HttpResponseData> CreateResponse(HttpRequestData httpRequestData, object data);
    }
}
