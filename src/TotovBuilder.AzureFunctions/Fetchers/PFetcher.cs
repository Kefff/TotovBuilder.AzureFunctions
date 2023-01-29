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

                    // Trying to add the following items as mods or content (content should always be the following item of its container)
                    // If not possible, we restart trying to add the following items as a mod from the topmost item
                    AddModSlots(inventoryItemModSlot.Item, moddableContainedItem, containedItems);
                    AddContent(inventoryItemModSlot.Item, containedItems);
                }
            }
            else
            {
                if (Items.FirstOrDefault(i => i.Id == inventoryItemModSlot.Item.ItemId) is IModdable alreadyPresentItem)
                {
                    AddModSlots(inventoryItemModSlot.Item, alreadyPresentItem, containedItems);
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
                InventoryItemModSlot? inventoryItemModSlot = inventoryItemModSlots.FirstOrDefault(iims => iims.ModSlotName == modSlot.Name);
                bool alreadyPreset = inventoryItemModSlot != null;

                if (!alreadyPreset)
                {
                    inventoryItemModSlot = new()
                    {
                        ModSlotName = modSlot.Name
                    };
                }

                AddModSlotItem(inventoryItemModSlot!, modSlot, containedItems);

                if (!alreadyPreset && inventoryItemModSlot!.Item != null)
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

            int tries = 1;

            while (containedItems.Count > 0)
            {
                if (tries > 10)
                {
                    throw new Exception(string.Format(Properties.Resources.PresetContructionError, presetId, containedItems.Peek().Item.Id));
                }

                AddModSlots(inventoryItem, baseItem, containedItems);
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
                IModdable baseItem = (IModdable)Items.First(i => i is IModdable && i.Id == baseItemId);
                IModdable presetItem = baseItem switch
                {
                    IArmorMod => DeserializeArmorModPreset(presetId, presetJson, (IArmorMod)baseItem),
                    IHeadwear => DeserializeHeadwearPreset(presetId, presetJson, (IHeadwear)baseItem),
                    IRangedWeapon => DeserializeRangedWeaponPreset(presetId, presetJson, propertiesJson, (IRangedWeapon)baseItem),
                    IRangedWeaponMod => DeserializeRangedWeaponModPreset(presetId, presetJson, (IRangedWeaponMod)baseItem),
                    IMod => DeserializeModPreset(presetId, presetJson, (IMod)baseItem),
                    _ => throw new NotSupportedException(),
                };

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

        /// <summary>
        /// Deserilizes the properties of a preset that are common between preset item categories.
        /// </summary>
        /// <typeparam name="T">Preset type.</typeparam>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="RangedWeapon"/> preset.</returns>
        private static T DeserializeBasePresetProperties<T>(string presetId, JsonElement presetJson, IModdable baseItem)
            where T : IModdable, new()
        {
            T preset = new()
            {
                CategoryId = baseItem.CategoryId,
                ConflictingItemIds = baseItem.ConflictingItemIds,
                IconLink = presetJson.GetProperty("iconLink").GetString()!,
                Id = presetId,
                ImageLink = presetJson.GetProperty("inspectImageLink").GetString()!,
                IsPreset = true,
                MarketLink = presetJson.GetProperty("link").GetString()!,
                MaxStackableAmount = baseItem.MaxStackableAmount,
                ModSlots = baseItem.ModSlots,
                Name = presetJson.GetProperty("name").GetString()!,
                ShortName = presetJson.GetProperty("shortName").GetString()!,
                Weight = baseItem.Weight,
                WikiLink = presetJson.GetProperty("wikiLink").GetString()!
            };

            return preset;
        }

        /// <summary>
        /// Deserializes an <see cref="ArmorMod"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="ArmorMod"/> preset.</returns>
        private static ArmorMod DeserializeArmorModPreset(string presetId, JsonElement presetJson, IArmorMod baseItem)
        {
            ArmorMod presetItem = DeserializeBasePresetProperties<ArmorMod>(presetId, presetJson, baseItem);

            presetItem.ArmorClass = baseItem.ArmorClass;
            presetItem.ArmoredAreas = baseItem.ArmoredAreas;
            presetItem.BlindnessProtectionPercentage = baseItem.BlindnessProtectionPercentage;
            presetItem.Durability = baseItem.Durability;
            presetItem.ErgonomicsPercentageModifier = baseItem.ErgonomicsPercentageModifier;
            presetItem.Material = baseItem.Material;
            presetItem.MovementSpeedPercentageModifier = baseItem.MovementSpeedPercentageModifier;
            presetItem.RicochetChance = baseItem.RicochetChance;
            presetItem.TurningSpeedPercentageModifier = baseItem.TurningSpeedPercentageModifier;

            return presetItem;
        }

        /// <summary>
        /// Deserializes an <see cref="Headwear"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="Headwear"/> preset.</returns>
        private static Headwear DeserializeHeadwearPreset(string presetId, JsonElement presetJson, IHeadwear baseItem)
        {
            Headwear presetItem = DeserializeBasePresetProperties<Headwear>(presetId, presetJson, baseItem);

            presetItem.ArmorClass = baseItem.ArmorClass;
            presetItem.ArmoredAreas = baseItem.ArmoredAreas;
            presetItem.BlocksHeadphones = baseItem.BlocksHeadphones;
            presetItem.Deafening = baseItem.Deafening;
            presetItem.Durability = baseItem.Durability;
            presetItem.ErgonomicsPercentageModifier = baseItem.ErgonomicsPercentageModifier;
            presetItem.Material = baseItem.Material;
            presetItem.MovementSpeedPercentageModifier = baseItem.MovementSpeedPercentageModifier;
            presetItem.RicochetChance = baseItem.RicochetChance;
            presetItem.TurningSpeedPercentageModifier = baseItem.TurningSpeedPercentageModifier;

            return presetItem;
        }

        /// <summary>
        /// Deserializes an <see cref="Mod"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="Mod"/> preset.</returns>
        private static Mod DeserializeModPreset(string presetId, JsonElement presetJson, IMod baseItem)
        {
            Mod presetItem = DeserializeBasePresetProperties<Mod>(presetId, presetJson, baseItem);

            presetItem.ErgonomicsModifier = baseItem.ErgonomicsModifier;

            return presetItem;
        }

        /// <summary>
        /// Deserializes a <see cref="RangedWeapon"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="propertiesJson">Json element representing the properties of the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="RangedWeapon"/> preset.</returns>
        private static RangedWeapon DeserializeRangedWeaponPreset(string presetId, JsonElement presetJson, JsonElement propertiesJson, IRangedWeapon baseItem)
        {
            RangedWeapon presetItem = DeserializeBasePresetProperties<RangedWeapon>(presetId, presetJson, baseItem);

            presetItem.Caliber = baseItem.Caliber;
            presetItem.Ergonomics = baseItem.Ergonomics;
            presetItem.FireModes = baseItem.FireModes;
            presetItem.FireRate = baseItem.FireRate;
            presetItem.HorizontalRecoil = baseItem.HorizontalRecoil;
            presetItem.VerticalRecoil = baseItem.VerticalRecoil;
            presetItem.MinuteOfAngle = propertiesJson.GetProperty("moa").GetDouble();

            return presetItem;
        }

        /// <summary>
        /// Deserializes an <see cref="RangedWeaponMod"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="RangedWeaponMod"/> preset.</returns>
        private static RangedWeaponMod DeserializeRangedWeaponModPreset(string presetId, JsonElement presetJson, IRangedWeaponMod baseItem)
        {
            RangedWeaponMod presetItem = DeserializeBasePresetProperties<RangedWeaponMod>(presetId, presetJson, baseItem);

            presetItem.ErgonomicsModifier = baseItem.ErgonomicsModifier;
            presetItem.RecoilPercentageModifier = baseItem.RecoilPercentageModifier;

            return presetItem;
        }
    }
}
