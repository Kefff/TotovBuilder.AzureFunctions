using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using TotovBuilder.AzureFunctions.Abstraction;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an items metadata fetcher.
    /// </summary>
    public class ItemsMetadataFetcher : ApiFetcher, IItemsMetadataFetcher
    {
        private readonly string _apiQueryKey;

        /// <inheritdoc/>
        protected override string ApiQueryKey => _apiQueryKey;
        
        /// <inheritdoc/>
        protected override DataType DataType => DataType.ItemsMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsMetadataFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public ItemsMetadataFetcher(ILogger<ItemsMetadataFetcher> logger, IHttpClientWrapperFactory httpClientWrapperFactory, IConfigurationReader configurationReader, ICache cache)
            : base(logger, httpClientWrapperFactory, configurationReader, cache)
        {
            _apiQueryKey = configurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.ApiItemsMetadataQueryKey);
        }
        
        /// <inheritdoc/>
        protected override Result<string> GetData(string responseContent)
        {
            try
            {
                JsonElement response = JsonDocument.Parse(responseContent).RootElement;

                if (!response.TryGetProperty("data", out JsonElement data))
                {
                    Logger.LogError(string.Format(Properties.Resources.InvalidApiResponseData, DataType.ToString(), responseContent));

                    return Result.Fail(string.Empty);
                }

                if (!data.TryGetProperty("itemsByType", out JsonElement items))
                {
                    Logger.LogError(string.Format(Properties.Resources.InvalidApiResponseData, DataType.ToString(), responseContent));
                
                    return Result.Fail(string.Empty);
                }

                string itemsAsJson = JsonSerializer.Serialize(items);

                if (string.IsNullOrWhiteSpace(itemsAsJson) || itemsAsJson == "\"\"" || itemsAsJson == "[]" || itemsAsJson == "{}")
                {
                    Logger.LogError(string.Format(Properties.Resources.InvalidApiResponseData, DataType.ToString(), responseContent));
                
                    return Result.Fail(string.Empty);
                }

                return Result.Ok(itemsAsJson);
            }
            catch (Exception)
            {
                Logger.LogError(string.Format(Properties.Resources.InvalidApiResponseData, DataType.ToString(), responseContent));

                return Result.Fail(string.Empty);
            }
        }
    }
}
