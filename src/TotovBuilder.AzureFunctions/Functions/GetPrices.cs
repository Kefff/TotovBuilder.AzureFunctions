using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
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
        /// <param name="bartersFetcher">Barters fetcher.</param>
        /// <param name="pricesFetcher">Prices fetcher.</param>
        public GetPrices(IBartersFetcher bartersFetcher, IPricesFetcher pricesFetcher)
        {
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
            List<Item> prices = new List<Item>(await PricesFetcher.Fetch() ?? Array.Empty<Item>());
            IEnumerable<Item> barters = await BartersFetcher.Fetch() ?? Array.Empty<Item>();

            foreach (Item barter in barters)
            {
                Item? price = prices.FirstOrDefault(p => p.Id == barter.Id);

                if (price != null)
                {
                    price.Prices = price.Prices.Concat(barter.Prices).ToArray();
                }
                else
                {
                    prices.Add(barter);
                }
            }

            return new OkObjectResult(prices);
        }
    }
}
