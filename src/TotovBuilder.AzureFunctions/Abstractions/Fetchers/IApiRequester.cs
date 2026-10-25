using System.Text.Json;
using FluentResults;
using TotovBuilder.AzureFunctions.Utils;

namespace TotovBuilder.AzureFunctions.Abstractions.Fetchers
{
    /// <summary>
    /// Provides the functionalities of a requester of API endpoints.
    /// </summary>
    public interface IApiRequester
    {
        /// <summary>
        /// Requests the <c>data</c> property of the response of an API endpoint.
        /// Each endpoint is requested only once ; following calls return the same data.
        /// </summary>
        /// <param name="path">Path of the endpoint relative to the API base URL.</param>
        /// <returns>Content of the <c>data</c> property of the endpoint response.</returns>
        Task<Result<JsonElement>> RequestData(string path);

        /// <summary>
        /// Requests an API translations endpoint.
        /// Each endpoint is requested only once ; following calls return the same translations.
        /// </summary>
        /// <param name="path">Path of the translations endpoint relative to the API base URL.</param>
        /// <returns>Translations.</returns>
        Task<Result<Translations>> RequestTranslations(string path);
    }
}
