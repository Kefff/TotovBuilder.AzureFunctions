using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Builds;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns presets to the caller.
    /// </summary>
    public class GetPresets
    {
        /// <summary>
        /// Azure Functions configuration reader.
        /// </summary>
        private readonly IAzureFunctionsConfigurationReader AzureFunctionsConfigurationReader;

        /// <summary>
        /// HTTP response data factory.
        /// </summary>
        private readonly IHttpResponseDataFactory HttpResponseDataFactory;

        /// <summary>
        /// Presets fetcher.
        /// </summary>
        private readonly IPresetsFetcher PresetsFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPresets"/> class.
        /// </summary>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="presetsFetcher">Presets fetcher.</param>
        public GetPresets(IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader,
            IHttpResponseDataFactory httpResponseDataFactory,
            IPresetsFetcher presetsFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
            HttpResponseDataFactory = httpResponseDataFactory;
            PresetsFetcher = presetsFetcher;
        }

        /// <summary>
        /// Gets the presets to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Presets.</returns>
        [Function("GetPresets")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "presets")] HttpRequestData httpRequest)
        {
            await AzureFunctionsConfigurationReader.Load();
            IEnumerable<InventoryItem> presets = await PresetsFetcher.Fetch();

            return await HttpResponseDataFactory.CreateEnumerableResponse(httpRequest, presets);
        }
    }
}
