using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an items fetcher.
    /// </summary>
    public class ItemsFetcher : ApiFetcher<Item[]>, IItemsFetcher
    {

        /// <inheritdoc/>
        protected override string ApiQueryKey => TotovBuilder.AzureFunctions.ConfigurationReader.ApiItemsQueryKey;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.Items;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public ItemsFetcher(ILogger logger, IHttpClientWrapperFactory httpClientWrapperFactory, IConfigurationReader configurationReader, ICache cache)
            : base(logger, httpClientWrapperFactory, configurationReader, cache)
        {
        }
        
        /// <inheritdoc/>
        protected override Result<Item[]> DeserializeData(string responseContent)
        {
            throw new NotImplementedException();
        }
    }
}
