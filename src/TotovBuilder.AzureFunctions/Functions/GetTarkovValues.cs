using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns values related to Tarkov gameplay to the caller.
    /// </summary>
    public class GetTarkovValues
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
        /// Tarkov values fetcher.
        /// </summary>
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetTarkovValues"/> class.
        /// </summary>
        /// <param name="configurationLoader">Configuration loader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="changelogFetcher">Tarkov values fetcher.</param>
        public GetTarkovValues(IConfigurationLoader configurationLoader,
            IHttpResponseDataFactory httpResponseDataFactory,
            ITarkovValuesFetcher changelogFetcher)
        {
            ConfigurationLoader = configurationLoader;
            HttpResponseDataFactory = httpResponseDataFactory;
            TarkovValuesFetcher = changelogFetcher;
        }

        /// <summary>
        /// Gets values related to Tarkov gameplay to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Values related to Tarkov gameplay.</returns>
        [Function("GetTarkovValues")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tarkovvalues")] HttpRequestData httpRequest)
        {
            await ConfigurationLoader.Load();
            TarkovValues tarkovValues = await TarkovValuesFetcher.Fetch();

            return await HttpResponseDataFactory.CreateResponse(httpRequest, tarkovValues);
        }
    }
}
