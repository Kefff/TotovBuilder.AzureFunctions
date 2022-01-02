using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstractions;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns market data to the caller.
    /// </summary>
    public class GetMarketData
    {
        /// <summary>
        /// Data fetcher.
        /// </summary>
        private readonly IDataFetcher DataFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetMarketData"/> class.
        /// </summary>
        /// <param name="dataFetcher">Data fetcher.</param>
        public GetMarketData(IDataFetcher dataFetcher)
        {
            DataFetcher = dataFetcher;
        }

        /// <summary>
        /// Gets the market data to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Market data.</returns>
        [FunctionName("GetMarketData")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "marketdata")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            string response = await DataFetcher.Fetch(DataType.MarketData);

            return new OkObjectResult(response);
        }
    }
}
