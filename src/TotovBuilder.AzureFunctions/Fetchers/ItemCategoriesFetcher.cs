using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Cache;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Cache;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an item categories fetcher.
    /// </summary>
    public class ItemCategoriesFetcher : StaticDataFetcher<IEnumerable<ItemCategory>>, IItemCategoriesFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName => ConfigurationWrapper.Values.AzureItemCategoriesBlobName;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.ItemCategories;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="cache">Cache.</param>
        public ItemCategoriesFetcher(
            ILogger<ItemCategoriesFetcher> logger,
            IBlobFetcher blobDataFetcher,
            IConfigurationWrapper configurationWrapper,
            ICache cache)
            : base(logger, blobDataFetcher, configurationWrapper, cache)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<ItemCategory>>> DeserializeData(string responseContent)
        {
            try
            {
                IEnumerable<ItemCategory> itemCategories = JsonSerializer.Deserialize<IEnumerable<ItemCategory>>(responseContent, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                })!;

                return Task.FromResult(Result.Ok(itemCategories));
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemCategoryDeserializationError, e);
                Logger.LogError(error);

                return Task.FromResult(Result.Fail<IEnumerable<ItemCategory>>(error));
            }
        }
    }
}
