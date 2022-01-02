using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstractions;

namespace TotovBuilder.AzureFunctions.Functions
{
    public class GetItemCategories
    {
        /// <summary>
        /// Data fetcher.
        /// </summary>
        private readonly IDataFetcher DataFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetItemCategories"/> class.
        /// </summary>
        /// <param name="dataFetcher">Data fetcher.</param>
        public GetItemCategories(IDataFetcher dataFetcher)
        {
            DataFetcher = dataFetcher;
        }

        [FunctionName("GetItemCategories")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "itemcategories")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            string response = await DataFetcher.Fetch(DataType.ItemCategories);

            return new OkObjectResult(response);
        }
    }
}
