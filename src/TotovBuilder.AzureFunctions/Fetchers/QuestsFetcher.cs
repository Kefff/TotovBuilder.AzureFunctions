using System;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a quests fetcher.
    /// </summary>
    public class QuestsFetcher : ApiFetcher<Quest[]>, IQuestsFetcher
    {
        private readonly string _apiQueryKey;

        /// <inheritdoc/>
        protected override string ApiQueryKey => _apiQueryKey;
        
        /// <inheritdoc/>
        protected override DataType DataType => throw new NotImplementedException();

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
            _apiQueryKey = configurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.ApiQuestsQueryKey);
        }
        
        /// <inheritdoc/>
        protected override Result<Quest[]> GetData(string responseContent)
        {
            throw new NotImplementedException();
        }
    }
}
