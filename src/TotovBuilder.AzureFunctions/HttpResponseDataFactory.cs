using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions;

namespace TotovBuilder.AzureFunctions
{
    /// <summary>
    /// Represents an HTTP response data factory.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HttpResponseDataFactory : IHttpResponseDataFactory
    {
        /// <inheritdoc/>
        public async Task<HttpResponseData> CreateResponse(HttpRequestData httpRequestData, object data)
        {
            HttpResponseData httpResponsetData = httpRequestData.CreateResponse(HttpStatusCode.OK);
            await httpResponsetData.WriteAsJsonAsync(data);

            return httpResponsetData;
        }
    }
}
