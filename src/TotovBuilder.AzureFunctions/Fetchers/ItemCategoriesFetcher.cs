using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an item categories fetcher.
    /// </summary>
    public class ItemCategoriesFetcher : StaticDataFetcher<IEnumerable<ItemCategory>>, IItemCategoriesFetcher
    {
        /// <inheritdoc/>
        protected override DataType DataType => DataType.ItemCategories;

        /// <inheritdoc/>
        protected override string AzureBlobName => _azureBlobName;
        private readonly string _azureBlobName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public ItemCategoriesFetcher(ILogger logger, IBlobFetcher blobDataFetcher, IConfigurationReader configurationReader, ICache cache)
            : base(logger, blobDataFetcher, configurationReader, cache)
        {
            _azureBlobName = ConfigurationReader.ReadString(TotovBuilder.AzureFunctions.ConfigurationReader.AzureItemCategoriesBlobNameKey);
        }
        
        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<ItemCategory>>> DeserializeData(string responseContent)
        {
            IEnumerable<ItemCategory> itemCategories = JsonSerializer.Deserialize<IEnumerable<ItemCategory>>(responseContent, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            return Task.FromResult(Result.Ok(itemCategories));
        }
    }
}
