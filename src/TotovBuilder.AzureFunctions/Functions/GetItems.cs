using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstraction;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns items to the caller.
    /// </summary>
    public class GetItems
    {
        /// <summary>
        /// Items fetcher.
        /// </summary>
        private readonly IItemsFetcher ItemsFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetItems"/> class.
        /// </summary>
        /// <param name="itemsFetcher">Items fetcher.</param>
        public GetItems(IItemsFetcher itemsFetcher)
        {
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
            string response = await ItemsFetcher.Fetch();

            return new OkObjectResult(response);
        }
    }
}
