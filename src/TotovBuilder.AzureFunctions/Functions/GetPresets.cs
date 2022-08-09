using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Builds;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns presets to the caller.
    /// </summary>
    public class GetPresets
    {
        /// <summary>
        /// Azure Functions configuration reader.
        /// </summary>
        private readonly IAzureFunctionsConfigurationReader AzureFunctionsConfigurationReader;

        /// <summary>
        /// Presets fetcher.
        /// </summary>
        private readonly IPresetsFetcher PresetsFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPresets"/> class.
        /// </summary>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="presetsFetcher">Presets fetcher.</param>
        public GetPresets(IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, IPresetsFetcher presetsFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
            PresetsFetcher = presetsFetcher;
        }

        /// <summary>
        /// Gets the presets to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Presets.</returns>
        [FunctionName("GetPresets")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "presets")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            await AzureFunctionsConfigurationReader.Load();
            IEnumerable<InventoryItem> presets = await PresetsFetcher.Fetch() ?? Array.Empty<InventoryItem>();

            return new OkObjectResult(presets);
        }
    }
}
