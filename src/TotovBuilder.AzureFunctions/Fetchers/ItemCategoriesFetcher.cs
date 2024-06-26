﻿using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Items;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an item categories fetcher.
    /// </summary>
    public class ItemCategoriesFetcher : RawDataFetcher<IEnumerable<ItemCategory>>, IItemCategoriesFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName
        {
            get
            {
                return ConfigurationWrapper.Values.RawItemCategoriesBlobName;
            }
        }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.ItemCategories;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCategoriesFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="azureBlobStorageManager">Azure blob storage manager.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        public ItemCategoriesFetcher(
            ILogger<ItemCategoriesFetcher> logger,
            IAzureBlobStorageManager azureBlobStorageManager,
            IConfigurationWrapper configurationWrapper)
            : base(logger, azureBlobStorageManager, configurationWrapper)
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
