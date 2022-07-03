using System;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a barters fetcher.
    /// </summary>
    public class BartersFetcher : ApiFetcher, IBartersFetcher
    {
        private readonly string _apiQueryKey;

        /// <inheritdoc/>
        protected override string ApiQueryKey => _apiQueryKey;
        
        /// <inheritdoc/>
        protected override DataType DataType => throw new NotImplementedException();

        /// <summary>
        /// Initializes a new instance of the <see cref="BartersFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public BartersFetcher(ILogger logger, IHttpClientWrapperFactory httpClientWrapperFactory, IConfigurationReader configurationReader, ICache cache)
            : base(logger, httpClientWrapperFactory, configurationReader, cache)
        {
            _apiQueryKey = configurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.ApiBartersQueryKey);
        }
        
        /// <inheritdoc/>
        protected override Result<string> GetData(string responseContent)
        {
            throw new NotImplementedException();
        }
    }
}
