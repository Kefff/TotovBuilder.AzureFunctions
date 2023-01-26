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
    public class PFetcher : ApiFetcher<IEnumerable<InventoryItem>>, IPFetcher
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
        /// Initializes a new instance of the <see cref="PFetcher"/> class.
        /// </summary>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="azureFunctionsConfigurationCache">Azure Functions configuration cache.</param>
        /// <param name="cache">Cache.</param>
        public PFetcher(
            ILogger<PFetcher> logger,
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
        /// <param name="containedItems">Contained item.</param>
        private void AddContent(InventoryItem inventoryItem, Queue<PresetContainedItem> containedItems)
        {
            if (containedItems.Count == 0)
            {
                return;
            }

            PresetContainedItem containedItem = containedItems.Peek();

            if (containedItem.Item is IModdable)
            {
                return;
            }
            else
            {
                if (containedItem.Item is IAmmunition)
                {
                    if (Items.FirstOrDefault(i => i.Id == inventoryItem.ItemId) is IMagazine magazine
                        && !magazine.AcceptedAmmunitionIds.Contains(containedItem.Item.Id))
                    {
                        return;
                    }
                }

                inventoryItem.Content = new InventoryItem[]
                {
                    new InventoryItem()
                    {
                        ItemId = containedItem.Item.Id,
                        Quantity = containedItem.Quantity
                    }
                };

                containedItems.Dequeue();
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

            IModdable moddableContainedItem = (IModdable)containedItem.Item;

            if (inventoryItemModSlot.Item == null)
            {
                if (modSlot.CompatibleItemIds.Any(ci => ci == moddableContainedItem.Id))
                {
                    inventoryItemModSlot.Item = new InventoryItem()
                    {
                        ItemId = moddableContainedItem.Id
                    };

                    if (containedItem.Quantity > 1)
                    {
                        containedItem.Quantity--;
                    }
                    else
                    {
                        containedItems.Dequeue();
                    }

                    AddModSlots(inventoryItemModSlot.Item, moddableContainedItem, containedItems);
                    AddContent(inventoryItemModSlot.Item, containedItems);
                }
            }
            else
            {
                if (Items.FirstOrDefault(i => i.Id == inventoryItemModSlot.Item.ItemId) is IModdable alreadyPresentItem)
                {
                    foreach (ModSlot alreadyPresentItemModSlot in alreadyPresentItem.ModSlots)
                    {
                        InventoryItemModSlot alreadyPresentItemInventoryModSlot = inventoryItemModSlot.Item.ModSlots.First(ms => ms.ModSlotName == alreadyPresentItemModSlot.Name);
                        AddModSlotItem(alreadyPresentItemInventoryModSlot, alreadyPresentItemModSlot, containedItems);
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

            List<InventoryItemModSlot> inventoryItemModSlots = new();

            foreach (ModSlot modSlot in item.ModSlots)
            {
                InventoryItemModSlot inventoryItemModSlot = new()
                {
                    ModSlotName = modSlot.Name
                };

                AddModSlotItem(inventoryItemModSlot, modSlot, containedItems);

                if (inventoryItemModSlot.Item != null)
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
        private InventoryItem? ConstructPreset(string presetId, IModdable baseItem, Queue<PresetContainedItem> containedItems)
        {
            InventoryItem inventoryItem = new()
            {
                ItemId = presetId
            };

            AddModSlots(inventoryItem, baseItem, containedItems);

            if (containedItems.Count > 0)
            {
                Logger.LogError(string.Format(Properties.Resources.PresetContructionError, presetId, string.Join("\", \"", containedItems.Select(ci => ci.Item.Id))));
                return null;
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
                InventoryItem? preset = DeserializeData(presetsJson);

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
        /// <returns>Deserialized item.</returns>
        private InventoryItem? DeserializeData(JsonElement presetJson)
        {
            if (TryDeserializeObject(presetJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                string presetId = presetJson.GetProperty("id").GetString()!;
                string baseItemId = propertiesJson.GetProperty("baseItem").GetProperty("id").GetString()!;
                IRangedWeapon baseItem = (IRangedWeapon)Items.First(i => i is RangedWeapon && i.Id == baseItemId);
                RangedWeapon presetItem = new()
                {
                    Caliber = baseItem.Caliber,
                    CategoryId = baseItem.CategoryId,
                    ConflictingItemIds = baseItem.ConflictingItemIds,
                    Ergonomics = baseItem.Ergonomics,
                    FireModes = baseItem.FireModes,
                    FireRate = baseItem.FireRate,
                    HorizontalRecoil = baseItem.HorizontalRecoil,
                    IconLink = presetJson.GetProperty("iconLink").GetString()!,
                    Id = presetId,
                    ImageLink = presetJson.GetProperty("inspectImageLink").GetString()!,
                    MarketLink = presetJson.GetProperty("link").GetString()!,
                    MaxStackableAmount = baseItem.MaxStackableAmount,
                    ModSlots = baseItem.ModSlots,
                    Name = presetJson.GetProperty("name").GetString()!,
                    ShortName = presetJson.GetProperty("shortName").GetString()!,
                    VerticalRecoil = baseItem.VerticalRecoil,
                    Weight = baseItem.Weight,
                    WikiLink = presetJson.GetProperty("wikiLink").GetString()!
                };

                double moa = propertiesJson.GetProperty("moa").GetDouble();

                if (moa > 0)
                {
                    presetItem.MinuteOfAngle = moa;
                }

                // TODO : NEED TO ADD THE PRESET TO THE LIST OF ITEMS
                // THIS MEANS THAT THE PFETCHER SHOULD BE CALLED IN THE ITEMSFETCHER OR IT SHOULD EXIST A MECHANISM ON THE ITEMSFETCHER TO ADD A NEW ITEM AND STORE IT IN THE CACHE

                Queue<PresetContainedItem> containedItems = new();

                foreach (JsonElement containedItemJson in presetJson.GetProperty("containsItems").EnumerateArray())
                {
                    string containedItemId = containedItemJson.GetProperty("item").GetProperty("id").GetString()!;

                    if (containedItemId == baseItemId)
                    {
                        // Skipping the first item which is the base item
                        continue;
                    }

                    Item containedItem = Items.First(i => i.Id == containedItemId);
                    int quantity = containedItemJson.GetProperty("quantity").GetInt32();

                    containedItems.Enqueue(new PresetContainedItem(containedItem, quantity));
                }

                InventoryItem? preset = ConstructPreset(presetId, baseItem, containedItems);

                return preset;
            }

            return null;
        }
    }
}
