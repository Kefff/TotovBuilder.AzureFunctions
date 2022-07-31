using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models.Builds;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a presets fetcher.
    /// </summary>
    public class PresetsFetcher : StaticDataFetcher<IEnumerable<InventoryItem>>, IPresetsFetcher
    {
        /// <inheritdoc/>
        protected override string AzureBlobName => AzureFunctionsConfigurationReader.Values.AzurePresetsBlobName;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.Presets;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresetsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="blobDataFetcher">Blob data fetcher.</param>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="cache">Cache.</param>
        public PresetsFetcher(ILogger logger, IBlobFetcher blobDataFetcher, IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, ICache cache)
            : base(logger, blobDataFetcher, azureFunctionsConfigurationReader, cache)
        {
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<InventoryItem>> DeserializeData(string responseContent)
        {
            List<InventoryItem> presetsResults = new List<InventoryItem>();

            JsonElement presetsJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement inventoryItemJson in presetsJson.EnumerateArray())
            {
                try
                {
                    presetsResults.Add(DeserializeInventoryItem(inventoryItemJson));
                }
                catch (Exception e)
                {
                    string error = string.Format(Properties.Resources.PresetDeserializationError, e);
                    Logger.LogError(error);
                }
            }

            return Task.FromResult(presetsResults.AsEnumerable());
        }

        /// <summary>
        /// Deserilizes an <see cref="InventoryItem"/>.
        /// </summary>
        /// <param name="inventoryItemJson">Json element representing the inventory item to deserialize.</param>
        /// <returns>Deserialized <see cref="InventoryItem"/>.</returns>
        private InventoryItem DeserializeInventoryItem(JsonElement inventoryItemJson)
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
                content.Add(DeserializeInventoryItem(contentInventoryItemJson));
            }

            inventoryItem.Content = content.ToArray();

            List<InventoryModSlot> modSlots = new List<InventoryModSlot>();

            foreach (JsonElement modSlotJson in inventoryItemJson.GetProperty("modSlots").EnumerateArray())
            {
                modSlots.Add(DeserializeInventoryModSlot(modSlotJson));
            }

            inventoryItem.ModSlots = modSlots.ToArray();

            return inventoryItem;
        }

        /// <summary>
        /// Deserilizes an <see cref="InventoryModSlot"/>.
        /// </summary>
        /// <param name="inventoryModSlotJson">Json element representing the inventory mod slot to deserialize.</param>
        /// <returns>Deserilized <see cref="InventoryModSlot"/>.</returns>
        private InventoryModSlot DeserializeInventoryModSlot(JsonElement inventoryModSlotJson)
        {
            InventoryModSlot inventoryModSlot = new InventoryModSlot()
            {
                Item = DeserializeInventoryItem(inventoryModSlotJson.GetProperty("item")),
                ModSlotName = inventoryModSlotJson.GetProperty("modSlotName").GetString()
            };

            return inventoryModSlot;
        }
    }
}
