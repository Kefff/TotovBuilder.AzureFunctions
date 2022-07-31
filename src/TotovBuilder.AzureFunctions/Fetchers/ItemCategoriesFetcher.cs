using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models.Items;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an item categories fetcher.
    /// </summary>
    public class ItemCategoriesFetcher : StaticDataFetcher<IEnumerable<ItemCategory>>, IItemCategoriesFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName => AzureFunctionsConfigurationReader.Values.AzureItemCategoriesBlobName;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.ItemCategories;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public ItemCategoriesFetcher(ILogger logger, IBlobFetcher blobDataFetcher, IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, ICache cache)
            : base(logger, blobDataFetcher, azureFunctionsConfigurationReader, cache)
        {
        }
        
        /// <inheritdoc/>
        protected override Task<IEnumerable<ItemCategory>> DeserializeData(string responseContent)
        {
            List<ItemCategory> itemCategories = new List<ItemCategory>();

            try
            {
                itemCategories.AddRange(JsonSerializer.Deserialize<IEnumerable<ItemCategory>>(responseContent, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                }));
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemCategoryDeserializationError, e);
                Logger.LogError(error);
            }

            return Task.FromResult(itemCategories.AsEnumerable());
        }
    }
}
