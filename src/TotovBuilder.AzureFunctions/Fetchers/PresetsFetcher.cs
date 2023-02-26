using System.Collections.Concurrent;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model;
using TotovBuilder.Model.Abstractions.Items;
using TotovBuilder.Model.Builds;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents a presets fetcher.
    /// </summary>
    public class PresetsFetcher : ApiFetcher<IEnumerable<InventoryItem>>, IPresetsFetcher
    {
        /// <inheritdoc/>
        protected override string ApiQuery => AzureFunctionsConfigurationCache.Values.ApiPresetsQuery;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.Presets;

        /// <summary>
        /// List of items.
        /// </summary>
        private IEnumerable<Item> Items = Array.Empty<Item>();

        /// <summary>
        /// Items fetcher.
        /// </summary>
        private readonly IItemsFetcher ItemsFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresetsFetcher"/> class.
        /// </summary>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="azureFunctionsConfigurationCache">Azure Functions configuration cache.</param>
        /// <param name="cache">Cache.</param>
        public PresetsFetcher(
            ILogger<PresetsFetcher> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IAzureFunctionsConfigurationCache azureFunctionsConfigurationCache,
            ICache cache,
            IItemsFetcher itemsFetcher
        ) : base(logger, httpClientWrapperFactory, azureFunctionsConfigurationCache, cache)
        {
            ItemsFetcher = itemsFetcher;
        }

        /// <inheritdoc/>
        protected override async Task<Result<IEnumerable<InventoryItem>>> DeserializeData(string responseContent)
        {
            List<Task> deserializationTasks = new();
            ConcurrentBag<InventoryItem> presets = new();
            Items = await ItemsFetcher.Fetch() ?? Array.Empty<Item>();

            JsonElement presetsJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement itemJson in presetsJson.EnumerateArray())
            {
                deserializationTasks.Add(Task.Run(() => DeserializeData(itemJson, presets)));
            }
            
            await Task.WhenAll(deserializationTasks);

            return Result.Ok(presets.AsEnumerable());
        }

        /// <summary>
        /// Adds the first of a list of contained items as the content of an inventory item if possible.
        /// </summary>
        /// <param name="inventoryItem">Inventory item.</param>
        /// <param name="item">Item.</param>
        /// <param name="containedItems">Contained item.</param>
        private static void AddContent(InventoryItem inventoryItem, IItem item, Queue<PresetContainedItem> containedItems)
        {
            if (containedItems.Count == 0)
            {
                return;
            }

            PresetContainedItem containedItem = containedItems.Peek();

            if (containedItem.Item is IAmmunition
                && item is IMagazine magazine
                && magazine.AcceptedAmmunitionIds.Contains(containedItem.Item.Id))
            {
                InventoryItem containedInventoryItem = new()
                {
                    ItemId = containedItem.Item.Id,
                    Quantity = containedItem.Quantity
                };
                inventoryItem.Content = new InventoryItem[]
                {
                    containedInventoryItem
                };

                containedItems.Dequeue();
                AddContent(containedInventoryItem, containedItem.Item, containedItems); // Continuing adding content while contained items are not moddable
            }
            else if (containedItem.Item is IAmmunition containedAmmunition
                && item is IAmmunition ammunition
                && ammunition.Caliber == containedAmmunition.Caliber)
            {
                // If the previous item is also ammunition of the same caliber, adding the quantity to the previous item because we only support
                // one ammunition type per magazine
                inventoryItem.Quantity += containedItem.Quantity;

                containedItems.Dequeue();
                AddContent(inventoryItem, item, containedItems); // Continuing adding content while contained items are not moddable
            }
            else if (containedItem.Item is not IModdable
                && item is not IMagazine
                && item is IContainer)
            {
                InventoryItem containedInventoryItem = new()
                {
                    ItemId = containedItem.Item.Id,
                    Quantity = containedItem.Quantity
                };
                inventoryItem.Content = inventoryItem.Content.Append(containedInventoryItem).ToArray();

                containedItems.Dequeue();
                AddContent(containedInventoryItem, containedItem.Item, containedItems); // Continuing adding content while contained items are not moddable
            }
        }

        /// <summary>
        /// Adds a contained item to an inventory item if possible.
        /// </summary>
        /// <param name="inventoryItem">Inventory item.</param>
        /// <param name="item">Item corresponding to the inventory item.</param>
        /// <param name="containedItems">Contained items.</param>
        private void AddItem(InventoryItem inventoryItem, IItem item, Queue<PresetContainedItem> containedItems)
        {
            if (containedItems.Count == 0)
            {
                return;
            }

            PresetContainedItem containedItem = containedItems.Peek();

            if (containedItem.Item is IModdable && item is IModdable moddable)
            {
                AddModSlots(inventoryItem, moddable, containedItems);
            }
            else
            {
                AddContent(inventoryItem, item, containedItems);
            }
        }

        /// <summary>
        /// Adds the first of a list of contained items as a modslot item of an inventory item if possible.
        /// </summary>
        /// <param name="inventoryItemModSlot">Inventory item modslot.</param>
        /// <param name="modSlot">Mod slot.</param>
        /// <param name="containedItems">Contained items.</param>
        private void AddModSlotItem(InventoryItemModSlot inventoryItemModSlot, ModSlot modSlot, Queue<PresetContainedItem> containedItems)
        {
            PresetContainedItem containedItem = containedItems.Peek();

            if (containedItem.Item is IModdable containedModdable)
            {
                if (inventoryItemModSlot.Item == null)
                {
                    if (!modSlot.CompatibleItemIds.Any(ci => ci == containedModdable.Id))
                    {
                        return;
                    }

                    inventoryItemModSlot.Item = new InventoryItem()
                    {
                        ItemId = containedModdable.Id
                    };

                    if (containedItem.Quantity > 1)
                    {
                        containedItem.Quantity--;
                    }
                    else
                    {
                        containedItems.Dequeue();
                    }

                    // Trying to add the following items as mods or content (content should always be the following item of its container)
                    // If not possible, we restart trying to add the following items as a mod from the topmost item
                    AddItem(inventoryItemModSlot.Item, containedModdable, containedItems);
                }
                else
                {
                    if (Items.FirstOrDefault(i => i.Id == inventoryItemModSlot.Item.ItemId) is IModdable alreadyPresentItem)
                    {
                        AddModSlots(inventoryItemModSlot.Item, alreadyPresentItem, containedItems);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the modslots and their contained item to an inventory item.
        /// </summary>
        /// <param name="inventoryItem">Inventory item.</param>
        /// <param name="item">Item corresponding to the inventory item.</param>
        /// <param name="containedItems">Contained items.</param>
        private void AddModSlots(InventoryItem inventoryItem, IModdable item, Queue<PresetContainedItem> containedItems)
        {
            if (item.ModSlots.Length == 0 || containedItems.Count == 0)
            {
                return;
            }

            List<InventoryItemModSlot> inventoryItemModSlots = new(inventoryItem.ModSlots);

            foreach (ModSlot modSlot in item.ModSlots)
            {
                if (containedItems.Count == 0)
                {
                    break;
                }

                InventoryItemModSlot? inventoryItemModSlot = inventoryItemModSlots.FirstOrDefault(iims => iims.ModSlotName == modSlot.Name);
                bool hasItem = inventoryItemModSlot != null; // Only modslots containing an item are added to inventoryItemModSlots 

                if (!hasItem)
                {
                    inventoryItemModSlot = new()
                    {
                        ModSlotName = modSlot.Name
                    };
                }

                AddModSlotItem(inventoryItemModSlot!, modSlot, containedItems);

                if (!hasItem && inventoryItemModSlot!.Item != null)
                {
                    // Only adding modslots containing an item
                    inventoryItemModSlots.Add(inventoryItemModSlot);
                }
            }

            inventoryItem.ModSlots = inventoryItemModSlots.ToArray();
        }

        /// <summary>
        /// Constructs a preset based on its base item and contained items.
        /// </summary>
        /// <param name="presetId">ID of the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <param name="containedItems">Contained items.</param>
        /// <returns>Preset.</returns>
        private InventoryItem ConstructPreset(string presetId, IModdable baseItem, Queue<PresetContainedItem> containedItems)
        {
            InventoryItem inventoryItem = new()
            {
                ItemId = presetId
            };

            int tries = 1;

            while (containedItems.Count > 0)
            {
                if (tries > 10)
                {
                    Logger.LogError(string.Format(Properties.Resources.PresetContructionError, presetId, containedItems.Peek().Item.Id));
                    break;
                }

                AddItem(inventoryItem, baseItem, containedItems);
                tries++;
            }

            return inventoryItem;
        }

        /// <summary>
        /// Deserializes data representing a preset into a list of presets.
        /// </summary>
        /// <param name="presetsJson">Json element representing the preset to deserialize.</param>
        /// <param name="presets">List of presets the deserialized preset will be stored into.</param>
        private void DeserializeData(JsonElement presetsJson, ConcurrentBag<InventoryItem> presets)
        {
            try
            {
                InventoryItem? preset = DeserializePresetData(presetsJson);

                if (preset != null)
                {
                    presets.Add(preset);
                }
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.PresetDeserializationError, e);
                Logger.LogError(error);
            }
        }

        /// <summary>
        /// Deserializes data representing a preset.
        /// </summary>
        /// <param name="presetJson">Json element representing the preset to deserialize.</param>
        /// <returns>Deserialized preset.</returns>
        private InventoryItem? DeserializePresetData(JsonElement presetJson)
        {
            string presetId = presetJson.GetProperty("id").GetString()!;

            Item? presetItem = Items.FirstOrDefault(i => i.Id == presetId);

            if (presetItem == null)
            {
                throw new InvalidDataException(string.Format(Properties.Resources.ItemNotFound, presetId));
            }
            else if (presetItem is not IModdable)
            {
                throw new InvalidDataException(string.Format(Properties.Resources.ItemNotModdable, presetId));
            }

            string baseItemId = ((IModdable)presetItem).BaseItemId!;
            Item? baseItem = Items.FirstOrDefault(i => i.Id == baseItemId);

            if (baseItem == null)
            {
                throw new InvalidDataException(string.Format(Properties.Resources.ItemNotFound, baseItemId));
            }
            else if (baseItem is not IModdable)
            {
                throw new InvalidDataException(string.Format(Properties.Resources.ItemNotModdable, baseItemId));
            }

            Queue<PresetContainedItem> containedItems = new();

            foreach (JsonElement containedItemJson in presetJson.GetProperty("containsItems").EnumerateArray())
            {
                string containedItemId = containedItemJson.GetProperty("item").GetProperty("id").GetString()!;

                if (containedItemId == baseItem.Id)
                {
                    // Skipping the first item which is the base item
                    continue;
                }

                Item containedItem = Items.First(i => i.Id == containedItemId);
                int quantity = containedItemJson.GetProperty("quantity").GetInt32();

                containedItems.Enqueue(new PresetContainedItem(containedItem, quantity));
            }

            InventoryItem preset = ConstructPreset(presetId, (IModdable)baseItem, containedItems);

            return preset;
        }
    }
}
