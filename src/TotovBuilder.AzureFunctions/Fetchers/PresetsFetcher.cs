using System;
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
    /// Represents a presets fetcher.
    /// </summary>
    public class PresetsFetcher : StaticDataFetcher<IEnumerable<InventoryItem>>, IPresetsFetcher
    {
        /// <inheritdoc/>
        protected override DataType DataType => DataType.Presets;

        /// <inheritdoc/>
        protected override string AzureBlobNameKey => TotovBuilder.AzureFunctions.ConfigurationReader.AzurePresetsBlobNameKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresetsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public PresetsFetcher(ILogger logger, IBlobFetcher blobDataFetcher, IConfigurationReader configurationReader, ICache cache)
            : base(logger, blobDataFetcher, configurationReader, cache)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<InventoryItem>>> DeserializeData(string responseContent)
        {
            try
            {
                List<Result<InventoryItem>> presetsResults = new List<Result<InventoryItem>>();
                JsonElement presetsJson = JsonDocument.Parse(responseContent).RootElement;

                foreach (JsonElement inventoryItemJson in presetsJson.EnumerateArray())
                {
                    presetsResults.Add(DeserializeInventoryItem(inventoryItemJson));
                }

                return Task.FromResult(Result.Merge(presetsResults.ToArray()));
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.PresetDeserializationError, e);
                Logger.LogError(error);

                return Task.FromResult(Result.Fail<IEnumerable<InventoryItem>>(error));
            }
        }

        /// <summary>
        /// Deserilizes an <see cref="InventoryItem"/>.
        /// </summary>
        /// <param name="inventoryItemJson">Json element representing the inventory item to deserialize.</param>
        /// <returns>Deserialized <see cref="InventoryItem"/>.</returns>
        private Result<InventoryItem> DeserializeInventoryItem(JsonElement inventoryItemJson)
        {
            try
            {
                InventoryItem inventoryItem = new InventoryItem()
                {
                    IgnorePrice = inventoryItemJson.GetProperty("ignorePrice").GetBoolean(),
                    ItemId = inventoryItemJson.GetProperty("itemId").GetString(),
                    Quantity = inventoryItemJson.GetProperty("quantity").GetDouble()
                };

                List<InventoryItem> content = new List<InventoryItem>();

                foreach (JsonElement contentInventoryItemJson in inventoryItemJson.GetProperty("content").EnumerateArray())
                {
                    Result<InventoryItem> contentInventoryItemResult = DeserializeInventoryItem(contentInventoryItemJson);

                    if (!contentInventoryItemResult.IsSuccess)
                    {
                        return contentInventoryItemResult;
                    }

                    content.Add(contentInventoryItemResult.Value);
                }

                inventoryItem.Content = content.ToArray();

                List<InventoryModSlot> modSlots = new List<InventoryModSlot>();

                foreach (JsonElement modSlotJson in inventoryItemJson.GetProperty("modSlots").EnumerateArray())
                {
                    Result<InventoryModSlot> modSlotResult = DeserializeInventoryModSlot(modSlotJson);

                    if (!modSlotResult.IsSuccess)
                    {
                        return modSlotResult.ToResult<InventoryItem>();
                    }

                    modSlots.Add(modSlotResult.Value);
                }

                inventoryItem.ModSlots = modSlots.ToArray();

                return Result.Ok(inventoryItem);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.PresetDeserializationError, e);
                Logger.LogError(error);

                return Result.Fail<InventoryItem>(error);
            }
        }

        /// <summary>
        /// Deserilizes an <see cref="InventoryModSlot"/>.
        /// </summary>
        /// <param name="inventoryModSlotJson">Json element representing the inventory mod slot to deserialize.</param>
        /// <returns>Deserilized <see cref="InventoryModSlot"/>.</returns>
        private Result<InventoryModSlot> DeserializeInventoryModSlot(JsonElement inventoryModSlotJson)
        {
            try
            {
                InventoryModSlot inventoryModSlot = new InventoryModSlot()
                {
                    ModSlotName = inventoryModSlotJson.GetProperty("modSlotName").GetString()
                };

                Result<InventoryItem> inventoryItemResult = DeserializeInventoryItem(inventoryModSlotJson.GetProperty("item"));

                if (!inventoryItemResult.IsSuccess)
                {
                    return inventoryItemResult.ToResult<InventoryModSlot>();
                }

                inventoryModSlot.Item = inventoryItemResult.Value;

                return Result.Ok(inventoryModSlot);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.PresetDeserializationError, e);
                Logger.LogError(error);

                return Result.Fail<InventoryModSlot>(error);
            }
        }
    }
}
