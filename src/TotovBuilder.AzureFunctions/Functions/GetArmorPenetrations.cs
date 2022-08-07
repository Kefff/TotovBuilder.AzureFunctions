using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns the armor penetrations to the caller.
    /// </summary>
    public class GetArmorPenetrations
    {
        /// <summary>
        /// Armor penetrations fetcher.
        /// </summary>
        private readonly IArmorPenetrationsFetcher ArmorPenetrationsFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetArmorPenetrations"/> class.
        /// </summary>
        /// <param name="changelogFetcher">Armor penetrations fetcher.</param>
        public GetArmorPenetrations(IArmorPenetrationsFetcher changelogFetcher)
        {
            ArmorPenetrationsFetcher = changelogFetcher;
        }

        /// <summary>
        /// Gets the armor penetrations to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Armor penetrations.</returns>
        [FunctionName("GetArmorPenetrations")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "armorpenetrations")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            IEnumerable<ArmorPenetration> armorPenetrations = await ArmorPenetrationsFetcher.Fetch() ?? Array.Empty<ArmorPenetration>();

            return new OkObjectResult(armorPenetrations);
        }
    }
}