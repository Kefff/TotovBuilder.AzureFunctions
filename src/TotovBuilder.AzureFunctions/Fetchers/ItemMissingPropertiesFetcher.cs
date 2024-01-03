using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a itemMissingProperties fetcher.
    /// </summary>
    public class ItemMissingPropertiesFetcher : RawDataFetcher<IEnumerable<ItemMissingProperties>>, IItemMissingPropertiesFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName
        {
            get
            {
                return ConfigurationWrapper.Values.RawItemMissingPropertiesBlobName;
            }
        }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.ItemMissingProperties;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemMissingPropertiesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureBlobStorageManager">Azure blob storage manager.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public ItemMissingPropertiesFetcher(
            ILogger<ItemMissingPropertiesFetcher> logger,
            IAzureBlobStorageManager azureBlobStorageManager,
            IConfigurationWrapper configurationWrapper)
            : base(logger, azureBlobStorageManager, configurationWrapper)
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

                return Task.FromResult(Result.Ok(missingItemProperties));
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemMissingPropertiesDeserializationError, e);
                Logger.LogError(error);

                return Task.FromResult(Result.Fail<IEnumerable<ItemMissingProperties>>(error));
            }
        }
    }
}
