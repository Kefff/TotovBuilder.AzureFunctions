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
    /// Represents an Azure function that returns values related to Tarkov gameplay to the caller.
    /// </summary>
    public class GetTarkovValues
    {
        /// <summary>
        /// Tarkov values fetcher.
        /// </summary>
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetTarkovValues"/> class.
        /// </summary>
        /// <param name="changelogFetcher">Tarkov values fetcher.</param>
        public GetTarkovValues(ITarkovValuesFetcher changelogFetcher)
        {
            TarkovValuesFetcher = changelogFetcher;
        }

        /// <summary>
        /// Gets values related to Tarkov gameplay to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Values related to Tarkov gameplay.</returns>
        [FunctionName("GetTarkovValues")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tarkovvalues")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            TarkovValues tarkovValues = await TarkovValuesFetcher.Fetch() ?? new TarkovValues();

            return new OkObjectResult(tarkovValues);
        }
    }
}
