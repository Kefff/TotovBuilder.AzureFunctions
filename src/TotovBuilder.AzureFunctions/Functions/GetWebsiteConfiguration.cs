using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns the website configuration to the caller.
    /// </summary>
    public class GetWebsiteConfiguration
    {
        /// <summary>
        /// Website configuration fetcher.
        /// </summary>
        private readonly IWebsiteConfigurationFetcher WebsiteConfigurationFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetWebsiteConfiguration"/> class.
        /// </summary>
        /// <param name="changelogFetcher">Website configuration fetcher.</param>
        public GetWebsiteConfiguration(IWebsiteConfigurationFetcher changelogFetcher)
        {
            WebsiteConfigurationFetcher = changelogFetcher;
        }

        /// <summary>
        /// Gets values related to Tarkov gameplay to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Values related to Tarkov gameplay.</returns>
        [FunctionName("GetWebsiteConfiguration")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tarkovvalues")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            WebsiteConfiguration websiteConfiguration = await WebsiteConfigurationFetcher.Fetch() ?? new WebsiteConfiguration();

            return new OkObjectResult(websiteConfiguration);
        }
    }
}
