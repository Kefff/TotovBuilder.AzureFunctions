using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns the website configuration to the caller.
    /// </summary>
    public class GetWebsiteConfiguration
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
        /// Website configuration fetcher.
        /// </summary>
        private readonly IWebsiteConfigurationFetcher WebsiteConfigurationFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetWebsiteConfiguration"/> class.
        /// </summary>
        /// <param name="configurationLoader">Configuration loader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="websiteConfigurationFetcher">Website configuration fetcher.</param>
        public GetWebsiteConfiguration(
            IConfigurationLoader configurationLoader,
            IHttpResponseDataFactory httpResponseDataFactory,
            IWebsiteConfigurationFetcher websiteConfigurationFetcher)
        {
            ConfigurationLoader = configurationLoader;
            HttpResponseDataFactory = httpResponseDataFactory;
            WebsiteConfigurationFetcher = websiteConfigurationFetcher;
        }

        /// <summary>
        /// Gets values related to Tarkov gameplay to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Values related to Tarkov gameplay.</returns>
        [Function("GetWebsiteConfiguration")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "websiteconfiguration")] HttpRequestData httpRequest)
        {
            await ConfigurationLoader.Load();
            WebsiteConfiguration websiteConfiguration = await WebsiteConfigurationFetcher.Fetch();

            return await HttpResponseDataFactory.CreateResponse(httpRequest, websiteConfiguration);
        }
    }
}
