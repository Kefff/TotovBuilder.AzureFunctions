using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns quests to the caller.
    /// </summary>
    public class GetQuests
    {
        /// <summary>
        /// Azure Functions configuration reader.
        /// </summary>
        private readonly IAzureFunctionsConfigurationReader AzureFunctionsConfigurationReader;

        /// <summary>
        /// Quests fetcher.
        /// </summary>
        private readonly IQuestsFetcher QuestsFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetQuests"/> class.
        /// </summary>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="questsFetcher">Quests fetcher.</param>
        public GetQuests(IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, IQuestsFetcher questsFetcher)
        {
            AzureFunctionsConfigurationReader = azureFunctionsConfigurationReader;
            QuestsFetcher = questsFetcher;
        }

        /// <summary>
        /// Gets the quests to return to the caller.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Prices.</returns>
        [FunctionName("GetQuests")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "quests")] HttpRequest httpRequest)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            await AzureFunctionsConfigurationReader.Load();
            IEnumerable<Quest> quests = await QuestsFetcher.Fetch() ?? Array.Empty<Quest>();

            return new OkObjectResult(quests);
        }
    }
}
