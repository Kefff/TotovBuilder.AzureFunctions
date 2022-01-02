using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using TotovBuilder.AzureFunctions.Abstractions;

namespace TotovBuilder.AzureFunctions.Functions
{
    public class GetPresets
    {
        /// <summary>
        /// Data fetcher.
        /// </summary>
        private readonly IDataFetcher DataFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPresets"/> class.
        /// </summary>
        /// <param name="dataFetcher">Data fetcher.</param>
        public GetPresets(IDataFetcher dataFetcher)
        {
            DataFetcher = dataFetcher;
        }

        [FunctionName("GetPresets")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "presets")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            string response = await DataFetcher.Fetch(DataType.Presets);

            return new OkObjectResult(response);
        }
    }
}
