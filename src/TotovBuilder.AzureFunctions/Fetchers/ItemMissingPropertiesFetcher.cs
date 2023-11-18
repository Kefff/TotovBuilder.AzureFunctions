using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Cache;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Cache;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a itemMissingProperties fetcher.
    /// </summary>
    public class ItemMissingPropertiesFetcher : StaticDataFetcher<IEnumerable<ItemMissingProperties>>, IItemMissingPropertiesFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName => ConfigurationWrapper.Values.AzureItemMissingPropertiesBlobName;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.ItemMissingProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemMissingPropertiesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="cache">Cache.</param>
        public ItemMissingPropertiesFetcher(
            ILogger<ItemMissingPropertiesFetcher> logger,
            IBlobFetcher blobDataFetcher,
            IConfigurationWrapper configurationWrapper,
            ICache cache)
            : base(logger, blobDataFetcher, configurationWrapper, cache)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<ItemMissingProperties>>> DeserializeData(string responseContent)
        {
            IEnumerable<ItemMissingProperties> missingItemProperties;

            try
            {
                missingItemProperties = JsonSerializer.Deserialize<IEnumerable<ItemMissingProperties>>(responseContent, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemMissingPropertiesDeserializationError, e);
                Logger.LogError(error);

                return Task.FromResult(Result.Fail<IEnumerable<ItemMissingProperties>>(error));
            }

            return Task.FromResult(Result.Ok(missingItemProperties));
        }
    }
}
