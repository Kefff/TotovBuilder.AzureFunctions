using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;

namespace TotovBuilder.AzureFunctions.Functions
{
    /// <summary>
    /// Represents an Azure function that returns quests to the caller.
    /// </summary>
    public class GetQuests
    {
        /// <summary>
        /// Data fetcher.
        /// </summary>
        private readonly IQuestsFetcher QuestsFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetItemCategories"/> class.
        /// </summary>
        /// <param name="dataFetcher">Data fetcher.</param>
        public GetQuests(IQuestsFetcher dataFetcher)
        {
            QuestsFetcher = dataFetcher;
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
            string response = await QuestsFetcher.Fetch();

            return new OkObjectResult(response);
        }
    }
}
