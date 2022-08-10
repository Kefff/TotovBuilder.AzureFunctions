using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
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
        /// Website configuration fetcher.
        /// </summary>
        private readonly IWebsiteConfigurationFetcher WebsiteConfigurationFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetWebsiteConfiguration"/> class.
        /// </summary>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="websiteConfigurationFetcher">Website configuration fetcher.</param>
        public GetWebsiteConfiguration(IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, IWebsiteConfigurationFetcher websiteConfigurationFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
            WebsiteConfigurationFetcher = websiteConfigurationFetcher;
        }

        /// <summary>
        /// Gets values related to Tarkov gameplay to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Values related to Tarkov gameplay.</returns>
        [FunctionName("GetWebsiteConfiguration")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "websiteconfiguration")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            await AzureFunctionsConfigurationReader.Load();
            WebsiteConfiguration websiteConfiguration = await WebsiteConfigurationFetcher.Fetch() ?? new WebsiteConfiguration();

            return new OkObjectResult(websiteConfiguration);
        }
    }
}
