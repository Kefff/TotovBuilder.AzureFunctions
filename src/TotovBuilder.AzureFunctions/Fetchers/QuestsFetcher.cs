using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a quests fetcher.
    /// </summary>
    public class QuestsFetcher : ApiFetcher<IEnumerable<Quest>>, IQuestsFetcher
    {
        /// <inheritdoc/>
        protected override string ApiQueryKey => TotovBuilder.AzureFunctions.ConfigurationReader.ApiQuestsQueryKey;
        
        /// <inheritdoc/>
        protected override DataType DataType => DataType.Quests;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public QuestsFetcher(ILogger logger, IHttpClientWrapperFactory httpClientWrapperFactory, IConfigurationReader configurationReader, ICache cache)
            : base(logger, httpClientWrapperFactory, configurationReader, cache)
        {
        }
        
        /// <inheritdoc/>
        protected override Task<IEnumerable<Quest>> DeserializeData(string responseContent)
        {
            List<Quest> quests = new List<Quest>();
            JsonElement questsJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement questJson in questsJson.EnumerateArray())
            {
                try
                {
                    quests.Add(new Quest()
                    {
                        Id = questJson.GetProperty("id").GetString(),
                        Name = questJson.GetProperty("name").GetString(),
                        Merchant = questJson.GetProperty("trader").GetProperty("normalizedName").GetString(),
                        WikiLink = questJson.GetProperty("wikiLink").GetString()
                    });
                }
                catch (Exception e)
                {
                    string error = string.Format(Properties.Resources.QuestDeserializationError, e);
                    Logger.LogError(error);
                }
            }

            return Task.FromResult(quests.AsEnumerable());
        }
    }
}
