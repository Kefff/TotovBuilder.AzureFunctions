using System;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a prices fetcher.
    /// </summary>
    public class PricesFetcher : ApiFetcher<Price[]>, IPricesFetcher
    {
        private readonly string _apiQueryKey;

        /// <inheritdoc/>
        protected override string ApiQueryKey => _apiQueryKey;
        
        /// <inheritdoc/>
        protected override DataType DataType => DataType.Prices;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public PricesFetcher(ILogger logger, IHttpClientWrapperFactory httpClientWrapperFactory, IConfigurationReader configurationReader, ICache cache)
            : base(logger, httpClientWrapperFactory, configurationReader, cache)
        {
            _apiQueryKey = configurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.ApiPricesQueryKey);
        }
        
        /// <inheritdoc/>
        protected override Result<Price[]> GetData(string responseContent)
        {
            throw new NotImplementedException();
        }
    }
}
