using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.Model.Builds;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns presets to the caller.
    /// </summary>
    public class GetPresets
    {
        /// <summary>
        /// Configuration loader.
        /// </summary>
        private readonly IConfigurationLoader ConfigurationLoader;

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
        /// <param name="configurationLoader">Configuration loader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="presetsFetcher">Presets fetcher.</param>
        public GetPresets(IConfigurationLoader configurationLoader,
            IHttpResponseDataFactory httpResponseDataFactory,
            IPresetsFetcher presetsFetcher)
        {
            ConfigurationLoader = configurationLoader;
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
            await ConfigurationLoader.Load();
            IEnumerable<InventoryItem> presets = await PresetsFetcher.Fetch();

            return await HttpResponseDataFactory.CreateEnumerableResponse(httpRequest, presets);
        }
    }
}
