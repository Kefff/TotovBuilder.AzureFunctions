using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns items to the caller.
    /// </summary>
    public class GetItems
    {
        /// <summary>
        /// Azure Functions configuration reader.
        /// </summary>
        private readonly IAzureFunctionsConfigurationReader AzureFunctionsConfigurationReader;

        /// <summary>
        /// Items fetcher.
        /// </summary>
        private readonly IItemsFetcher ItemsFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetItems"/> class.
        /// </summary>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="itemsFetcher">Items fetcher.</param>
        public GetItems(IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, IItemsFetcher itemsFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
            ItemsFetcher = itemsFetcher;
        }

        /// <summary>
        /// Gets the items to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Items.</returns>
        [FunctionName("GetItems")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "items")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            await AzureFunctionsConfigurationReader.Load();
            IEnumerable<Item> items = await ItemsFetcher.Fetch() ?? Array.Empty<Item>();

            return new OkObjectResult(items);
        }
    }
}
