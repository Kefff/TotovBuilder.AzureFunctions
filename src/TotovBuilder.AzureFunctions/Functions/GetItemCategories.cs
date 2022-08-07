using System;
using System.Collections.Generic;
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
    /// Represents an Azure function that returns item categories to the caller.
    /// </summary>
    public class GetItemCategories
    {
        /// <summary>
        /// Item categories fetcher.
        /// </summary>
        private readonly IItemCategoriesFetcher ItemCategoriesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetItemCategories"/> class.
        /// </summary>
        /// <param name="itemCategoriesFetcher">Item categories fetcher.</param>
        public GetItemCategories(IItemCategoriesFetcher itemCategoriesFetcher)
        {
            ItemCategoriesFetcher = itemCategoriesFetcher;
        }

        /// <summary>
        /// Gets the item categories to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Item categories.</returns>
        [FunctionName("GetItemCategories")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "itemcategories")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            IEnumerable<ItemCategory> itemCategories = await ItemCategoriesFetcher.Fetch() ?? Array.Empty<ItemCategory>();

            return new OkObjectResult(itemCategories);
        }
    }
}
