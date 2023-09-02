using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns the website configuration to the caller.
    /// </summary>
    public class GetWebsiteConfiguration
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
        /// Website configuration fetcher.
        /// </summary>
        private readonly IWebsiteConfigurationFetcher WebsiteConfigurationFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetWebsiteConfiguration"/> class.
        /// </summary>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="httpResponseDataFactory">Http response data factory.</param>
        /// <param name="websiteConfigurationFetcher">Website configuration fetcher.</param>
        public GetWebsiteConfiguration(
            IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader,
            IHttpResponseDataFactory httpResponseDataFactory,
            IWebsiteConfigurationFetcher websiteConfigurationFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
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
            await AzureFunctionsConfigurationReader.Load();
            WebsiteConfiguration websiteConfiguration = await WebsiteConfigurationFetcher.Fetch();

            return await HttpResponseDataFactory.CreateResponse(httpRequest, websiteConfiguration);
        }
    }
}
