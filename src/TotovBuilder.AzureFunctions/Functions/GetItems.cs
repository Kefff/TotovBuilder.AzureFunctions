using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstractions;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns items to the caller.
    /// </summary>
    public class GetItems
    {
        /// <summary>
        /// Data fetcher.
        /// </summary>
        private readonly IDataFetcher DataFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetItems"/> class.
        /// </summary>
        /// <param name="dataFetcher">Data fetcher.</param>
        public GetItems(IDataFetcher dataFetcher)
        {
            DataFetcher = dataFetcher;
        }

        /// <summary>
        /// Gets the items to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <param name="logger">Logger.</param>
        /// <returns>Items.</returns>
        [FunctionName("GetItems")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "items")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            string response = await DataFetcher.Fetch(DataType.Items);

            return new OkObjectResult(response);
        }
    }
}
