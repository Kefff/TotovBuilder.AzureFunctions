using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Represents an Azure function that returns prices to the caller.
    /// </summary>
    public class GetPrices
    {
        /// <summary>
        /// Azure Functions configuration reader.
        /// </summary>
        private readonly IAzureFunctionsConfigurationReader AzureFunctionsConfigurationReader;

        /// <summary>
        /// Barters fetcher.
        /// </summary>
        private readonly IBartersFetcher BartersFetcher;

        /// <summary>
        /// Prices fetcher.
        /// </summary>
        private readonly IPricesFetcher PricesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPrices"/> class.
        /// </summary>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="bartersFetcher">Barters fetcher.</param>
        /// <param name="pricesFetcher">Prices fetcher.</param>
        public GetPrices(IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, IBartersFetcher bartersFetcher, IPricesFetcher pricesFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
            BartersFetcher = bartersFetcher;
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
            await AzureFunctionsConfigurationReader.Load();

            IEnumerable<Price> prices = await PricesFetcher.Fetch() ?? Array.Empty<Price>();
            IEnumerable<Price> barters = await BartersFetcher.Fetch() ?? Array.Empty<Price>();

            return new OkObjectResult(prices.Concat(barters).OrderBy(p => p.ItemId));
        }
    }
}
