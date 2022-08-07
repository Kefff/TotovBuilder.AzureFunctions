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
    /// Represents an Azure function that returns the changelog to the caller.
    /// </summary>
    public class GetChangelog
    {
        /// <summary>
        /// Changelog fetcher.
        /// </summary>
        private readonly IChangelogFetcher ChangelogFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetChangelog"/> class.
        /// </summary>
        /// <param name="changelogFetcher">Changelog fetcher.</param>
        public GetChangelog(IChangelogFetcher changelogFetcher)
        {
            ChangelogFetcher = changelogFetcher;
        }

        /// <summary>
        /// Gets the changelog to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Changelog.</returns>
        [FunctionName("GetChangelog")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "changelog")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            IEnumerable<ChangelogEntry> changelog = await ChangelogFetcher.Fetch() ?? Array.Empty<ChangelogEntry>();

            return new OkObjectResult(changelog);
        }
    }
}
