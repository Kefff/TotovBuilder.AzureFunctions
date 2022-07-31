using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models.Items;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns prices to the caller.
    /// </summary>
    public class GetPrices
    {
        /// <summary>
        /// Data fetcher.
        /// </summary>
        private readonly IPricesFetcher PricesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPrices"/> class.
        /// </summary>
        /// <param name="pricesFetcher">Prices fetcher.</param>
        public GetPrices(IPricesFetcher pricesFetcher)
        {
            PricesFetcher = pricesFetcher;
        }

        /// <summary>
        /// Gets the prices to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Prices.</returns>
        [FunctionName("GetPrices")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "prices")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            IEnumerable<Item> response = await PricesFetcher.Fetch() ?? Array.Empty<Item>();

            return new OkObjectResult(response);
        }
    }
}
