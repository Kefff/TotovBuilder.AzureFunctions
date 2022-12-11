using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Builds;

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
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration wrapper.</param>
        /// <param name="cache">Cache.</param>
        public PresetsFetcher(ILogger<PresetsFetcher> logger, IBlobFetcher blobDataFetcher, IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, ICache cache)
            : base(logger, blobDataFetcher, azureFunctionsConfigurationReader, cache)
        {
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<InventoryItem>>> DeserializeData(string responseContent)
        {
            List<InventoryItem> presets = new();
            JsonElement presetsJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement inventoryItemJson in presetsJson.EnumerateArray())
            {
                try
                {
                    presets.Add(DeserializeInventoryItem(inventoryItemJson));
                }
                catch (Exception e)
                {
                    string error = string.Format(Properties.Resources.PresetDeserializationError, e);
                    Logger.LogError(error);
                }
            }

            return Task.FromResult(Result.Ok(presets.AsEnumerable()));
        }

        /// <summary>
        /// Deserilizes an <see cref="InventoryItem"/>.
        /// </summary>
        /// <param name="inventoryItemJson">Json element representing the inventory item to deserialize.</param>
        /// <returns>Deserialized <see cref="InventoryItem"/>.</returns>
        private InventoryItem DeserializeInventoryItem(JsonElement inventoryItemJson)
        {
            InventoryItem inventoryItem = new()
            {
                IgnorePrice = inventoryItemJson.GetProperty("ignorePrice").GetBoolean(),
                ItemId = inventoryItemJson.GetProperty("itemId").GetString()!,
                Quantity = inventoryItemJson.GetProperty("quantity").GetDouble()
            };

            List<InventoryItem> content = new();

            foreach (JsonElement contentInventoryItemJson in inventoryItemJson.GetProperty("content").EnumerateArray())
            {
                content.Add(DeserializeInventoryItem(contentInventoryItemJson));
            }

            inventoryItem.Content = content.ToArray();

            List<InventoryItemModSlot> modSlots = new();

            foreach (JsonElement modSlotJson in inventoryItemJson.GetProperty("modSlots").EnumerateArray())
            {
                modSlots.Add(DeserializeInventoryModSlot(modSlotJson));
            }

            inventoryItem.ModSlots = modSlots.ToArray();

            return inventoryItem;
        }

        /// <summary>
        /// Deserilizes an <see cref="InventoryItemModSlot"/>.
        /// </summary>
        /// <param name="inventoryModSlotJson">Json element representing the inventory mod slot to deserialize.</param>
        /// <returns>Deserilized <see cref="InventoryItemModSlot"/>.</returns>
        private InventoryItemModSlot DeserializeInventoryModSlot(JsonElement inventoryModSlotJson)
        {
            InventoryItemModSlot inventoryModSlot = new()
            {
                Item = DeserializeInventoryItem(inventoryModSlotJson.GetProperty("item")),
                ModSlotName = inventoryModSlotJson.GetProperty("modSlotName").GetString()!
            };

            return inventoryModSlot;
        }
    }
}
