using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions.Net;

namespace TotovBuilder.AzureFunctions.Net
{
    /// <summary>
    /// Represents an HTTP response data factory.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HttpResponseDataFactory : IHttpResponseDataFactory
    {
        /// <inheritdoc/>
        public async Task<HttpResponseData> CreateEnumerableResponse(HttpRequestData httpRequestData, IEnumerable<object> data)
        {
            string jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            HttpResponseData httpResponsetData = httpRequestData.CreateResponse(HttpStatusCode.OK);
            await httpResponsetData.WriteStringAsync(jsonData);

            return httpResponsetData;
        }

        /// <inheritdoc/>
        public async Task<HttpResponseData> CreateResponse(HttpRequestData httpRequestData, object data)
        {
            string jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            HttpResponseData httpResponsetData = httpRequestData.CreateResponse(HttpStatusCode.OK);
            await httpResponsetData.WriteStringAsync(jsonData);

            return httpResponsetData;
        }
    }
}
