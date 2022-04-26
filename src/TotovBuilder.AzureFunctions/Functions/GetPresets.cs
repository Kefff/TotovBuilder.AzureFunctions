using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using TotovBuilder.AzureFunctions.Abstraction;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns presets to the caller.
    /// </summary>
    public class GetPresets
    {
        /// <summary>
        /// Data fetcher.
        /// </summary>
        private readonly IPresetsFetcher PresetsFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPresets"/> class.
        /// </summary>
        /// <param name="presetsFetcher">Presets fetcher.</param>
        public GetPresets(IPresetsFetcher presetsFetcher)
        {
            PresetsFetcher = presetsFetcher;
        }

        /// <summary>
        /// Gets the presets to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Items.</returns>
        [FunctionName("GetPresets")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "presets")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            string response = await PresetsFetcher.Fetch();

            return new OkObjectResult(response);
        }
    }
}
