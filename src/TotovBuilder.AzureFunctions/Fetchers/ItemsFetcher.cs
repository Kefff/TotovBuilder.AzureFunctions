using System.Collections.Concurrent;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Abstractions.Items;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using static System.Text.Json.JsonElement;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an items fetcher.
    /// </summary>
    public class ItemsFetcher : ApiFetcher<IEnumerable<Item>>, IItemsFetcher
    {
        /// <inheritdoc/>
        protected override string ApiQuery
        {
            get
            {
                return ConfigurationWrapper.Values.ApiItemsQuery;
            }
        }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Items;
            }
        }

        /// <summary>
        /// List of item categories.
        /// </summary>
        private IEnumerable<ItemCategory> ItemCategories = Array.Empty<ItemCategory>();

        /// <summary>
        /// Item categories fetcher.
        /// </summary>
        private readonly IItemCategoriesFetcher ItemCategoriesFetcher;

        /// <summary>
        /// List of item missing properties to complete fetched items with.
        /// </summary>
        private IEnumerable<ItemMissingProperties> ItemMissingProperties = Array.Empty<ItemMissingProperties>();

        /// <summary>
        /// Item missing properties fetcher.
        /// </summary>
        private readonly IItemMissingPropertiesFetcher ItemMissingPropertiesFetcher;

        /// <summary>
        /// Tarkov values fetcher.
        /// </summary>
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <summary>
        /// Tarkov values.
        /// </summary>
        private TarkovValues TarkovValues = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="itemCategoriesFetcher">Item categories fetcher.</param>
        /// <param name="itemMissingPropertiesFetcher">Item missing properties fetcher.</param>
        /// <param name="tarkovValuesFetcher">Tarkov values fetcher.</param>
        public ItemsFetcher(
            ILogger<ItemsFetcher> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IConfigurationWrapper configurationWrapper,
            IItemCategoriesFetcher itemCategoriesFetcher,
            IItemMissingPropertiesFetcher itemMissingPropertiesFetcher,
            ITarkovValuesFetcher tarkovValuesFetcher
        ) : base(logger, httpClientWrapperFactory, configurationWrapper)
        {
            ItemCategoriesFetcher = itemCategoriesFetcher;
            ItemMissingPropertiesFetcher = itemMissingPropertiesFetcher;
            TarkovValuesFetcher = tarkovValuesFetcher;
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<Item>>> DeserializeData(string responseContent)
        {
            Result? itemCategoriesResult = null;
            Result? itemMissingPropertiesResult = null;
            Result? tarkovValuesResult = null;

            Task.WaitAll(
                ItemCategoriesFetcher.Fetch().ContinueWith(r => itemCategoriesResult = r.Result),
                ItemMissingPropertiesFetcher.Fetch().ContinueWith(r => itemMissingPropertiesResult = r.Result),
                TarkovValuesFetcher.Fetch().ContinueWith(r => tarkovValuesResult = r.Result));
            Result allTasksResult = Result.Merge(itemCategoriesResult, itemMissingPropertiesResult, tarkovValuesResult);

            if (allTasksResult.IsSuccess)
            {
                ItemCategories = ItemCategoriesFetcher.FetchedData!;
                ItemMissingProperties = ItemMissingPropertiesFetcher.FetchedData!;
                TarkovValues = TarkovValuesFetcher.FetchedData!;
            }
            else
            {
                return Task.FromResult(allTasksResult.ToResult<IEnumerable<Item>>());
            }

            List<Task> deserializationTasks = [];
            ConcurrentBag<Item> items = [];
            JsonElement itemsJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement itemJson in itemsJson.EnumerateArray())
            {
                deserializationTasks.Add(Task.Run(() => DeserializeData(DataDeserializationType.Item, itemJson, items)));
            }

            Task.WaitAll([.. deserializationTasks]);
            deserializationTasks.Clear();

            foreach (JsonElement itemJson in itemsJson.EnumerateArray())
            {
                deserializationTasks.Add(Task.Run(() => DeserializeData(DataDeserializationType.Preset, itemJson, items)));
            }

            Task.WaitAll([.. deserializationTasks]);

            return Task.FromResult(Result.Ok(items.AsEnumerable()));
        }

        /// <summary>
        /// Deserilizes <see cref="Ammunition"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Ammunition"/>.</returns>
        private Ammunition DeserializeAmmunition(JsonElement itemJson, string itemCategoryId)
        {
            Ammunition ammunition = DeserializeBaseItemProperties<Ammunition>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                ammunition.AccuracyModifierPercentage = propertiesJson.GetProperty("accuracyModifier").GetDouble();
                ammunition.ArmorDamagePercentage = propertiesJson.GetProperty("armorDamage").GetDouble() / 100;
                //ammunition.Blinding = ; // TODO : MISSING FROM API
                ammunition.Caliber = propertiesJson.GetProperty("caliber").GetString()!;
                ammunition.DurabilityBurnModifierPercentage = Math.Round(propertiesJson.GetProperty("durabilityBurnFactor").GetDouble() - 1, 2);
                ammunition.FleshDamage = propertiesJson.GetProperty("damage").GetDouble();
                ammunition.FragmentationChance = propertiesJson.GetProperty("fragmentationChance").GetDouble();
                ammunition.HeavyBleedingChance = propertiesJson.GetProperty("heavyBleedModifier").GetDouble();
                ammunition.LightBleedingChance = propertiesJson.GetProperty("lightBleedModifier").GetDouble();
                ammunition.MaxStackableAmount = propertiesJson.GetProperty("stackMaxSize").GetDouble();
                ammunition.PenetrationPower = propertiesJson.GetProperty("penetrationPower").GetDouble();
                ammunition.Projectiles = propertiesJson.GetProperty("projectileCount").GetDouble();
                ammunition.RecoilModifier = Math.Round(propertiesJson.GetProperty("recoilModifier").GetDouble() * 100); // The API returns it has a percentage but in reality it is a flat value added to the weapon recoil before mods
                ammunition.Tracer = propertiesJson.GetProperty("tracer").GetBoolean();
                ammunition.Velocity = propertiesJson.GetProperty("initialSpeed").GetDouble();

                ammunition.PenetratedArmorLevel = Math.Min(Math.Floor(ammunition.PenetrationPower / 10), TarkovValues.MaxArmorLevel); // Cf. https://youtu.be/8DUN-5--xes?feature=shared&t=141
                ammunition.Subsonic = ammunition.Velocity < 343; // Speed of sound in the air at 20°C at sea level
            }

            return ammunition;
        }

        /// <summary>
        /// Deserilizes an <see cref="Armor"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Armor"/>.</returns>
        private T DeserializeArmor<T>(JsonElement itemJson, string itemCategoryId)
            where T : Item, IArmor, new()
        {
            T armor = DeserializeBaseItemProperties<T>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                if (TryDeserializeDouble(propertiesJson, "blindnessProtection", out double blindnessProtection))
                {
                    armor.BlindnessProtectionPercentage = blindnessProtection;
                }

                if (TryDeserializeDouble(propertiesJson, "class", out double armorClass))
                {
                    // For armors with a soft armor, this value will be overridden by the front soft armor armor class
                    armor.ArmorClass = armorClass;
                }

                if (TryDeserializeDouble(propertiesJson, "durability", out double durability))
                {
                    // For armors with a soft armor, this value will be overridden by the sum of the soft armor durability
                    armor.Durability = durability;
                }

                if (TryDeserializeDouble(propertiesJson, "ergoPenalty", out double ergonomicsModifierPercentage))
                {
                    armor.ErgonomicsModifierPercentage = ergonomicsModifierPercentage;
                }

                if (TryDeserializeObject(propertiesJson, "material", out JsonElement materialJson))
                {
                    armor.Material = materialJson.GetProperty("id").GetString()!;
                }

                if (TryDeserializeDouble(propertiesJson, "speedPenalty", out double movementSpeedModifierPercentage))
                {
                    armor.MovementSpeedModifierPercentage = movementSpeedModifierPercentage;
                }

                if (TryDeserializeDouble(propertiesJson, "turnPenalty", out double turningSpeedModifierPercentage))
                {
                    armor.TurningSpeedModifierPercentage = turningSpeedModifierPercentage;
                }

                DeserializeArmorModSlots(armor, propertiesJson);

                // MISSING FROM API : Armored items should have a "defaultPreset". For now, we set it when deserializing a preset and we find an item that has the same name minus " Default"
                //if (TryDeserializeObject(propertiesJson, "defaultPreset", out JsonElement defaultPresetJson))
                //{
                //    armor.DefaultPresetId = defaultPresetJson.GetProperty("id").GetString();
                //}
            }

            return armor;
        }

        /// <summary>
        /// Deserializes an <see cref="Armor"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="Armor"/> preset.</returns>
        private static T DeserializeArmorPreset<T>(string presetId, JsonElement presetJson, IArmor baseItem)
            where T : Item, IArmor, new()
        {
            T presetItem = DeserializeBasePresetProperties<T>(presetId, presetJson, baseItem);

            presetItem.ArmorClass = baseItem.ArmorClass;
            presetItem.ArmoredAreas = baseItem.ArmoredAreas;
            presetItem.BlindnessProtectionPercentage = baseItem.BlindnessProtectionPercentage;
            presetItem.Durability = baseItem.Durability;
            presetItem.ErgonomicsModifierPercentage = baseItem.ErgonomicsModifierPercentage;
            presetItem.Material = baseItem.Material;
            presetItem.MovementSpeedModifierPercentage = baseItem.MovementSpeedModifierPercentage;
            presetItem.TurningSpeedModifierPercentage = baseItem.TurningSpeedModifierPercentage;

            if (presetItem.Name.EndsWith(" Default"))
            {
                baseItem.DefaultPresetId = presetItem.Id; // MISSING FROM API
            }

            return presetItem;
        }

        /// <summary>
        /// Deserilizes an <see cref="ArmorMod"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="ArmorMod"/>.</returns>
        private ArmorMod DeserializeArmorMod(JsonElement itemJson, string itemCategoryId)
        {
            ArmorMod armorMod = DeserializeArmor<ArmorMod>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                ModSlot[] modsSlots = DeserializeModSlots(propertiesJson);
                armorMod.ModSlots = [.. armorMod.ModSlots, .. modsSlots];

                if (TryDeserializeArray(propertiesJson, "zones", out ArrayEnumerator headArmoredAreasJson))
                {
                    armorMod.ArmoredAreas = [.. headArmoredAreasJson.Select(z => GetArmoredAreaName(z)).Distinct()];
                }
            }

            return armorMod;
        }

        /// <summary>
        /// Deserializes an array of <see cref="ArmorModSlot"/> and updates an <see cref="IArmor"/> with the deserialized values.
        /// </summary>
        /// <param name="item">Item to update with deserialized data.</param>
        /// <param name="propertiesJson">Json element representing the properties of an item.</param>
        private void DeserializeArmorModSlots(IArmor item, JsonElement propertiesJson)
        {
            if (!TryDeserializeArray(propertiesJson, "armorSlots", out ArrayEnumerator armorModSlotsJson))
            {
                return;
            }

            List<string> armoredAreas = [];
            List<ModSlot> armorModSlots = [];
            int lockedArmorSlotsDurability = 0;

            foreach (JsonElement modSlotJson in armorModSlotsJson)
            {
                string armorSlotType = modSlotJson.GetProperty("__typename").GetString()!;

                if (armorSlotType == "ItemArmorSlotOpen")
                {
                    ModSlot armorModSlot = new()
                    {
                        CompatibleItemIds = modSlotJson.GetProperty("allowedPlates").EnumerateArray().Select(ai => ai.GetProperty("id").GetString()!).ToArray(),
                        Name = modSlotJson.GetProperty("nameId").GetString()!.ToLowerInvariant()
                    };
                    armorModSlots.Add(armorModSlot);
                }
                else if (armorSlotType == "ItemArmorSlotLocked")
                {
                    string armorSlotName = modSlotJson.GetProperty("nameId").GetString()!;

                    if (armorSlotName == "Soft_armor_front")
                    {
                        item.ArmorClass = modSlotJson.GetProperty("class").GetInt32();
                    }
                    
                    lockedArmorSlotsDurability += modSlotJson.GetProperty("durability").GetInt32();
                }

                IEnumerable<string> zones = modSlotJson
                    .GetProperty("zones")
                    .EnumerateArray()
                    .Select(GetArmoredAreaName);
                armoredAreas.AddRange(zones);
            }

            if (lockedArmorSlotsDurability > 0)
            {
                item.Durability = lockedArmorSlotsDurability;
            }

            item.ArmoredAreas = [.. armoredAreas.Distinct()];
            item.ModSlots = [.. item.ModSlots, .. armorModSlots];
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
            ArmorMod presetItem = DeserializeArmorPreset<ArmorMod>(presetId, presetJson, baseItem);

            presetItem.BlindnessProtectionPercentage = baseItem.BlindnessProtectionPercentage;

            return presetItem;
        }

        /// <summary>
        /// Deserilizes the properties of an item that are common between item categories.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Item"/>.</returns>
        private T DeserializeBaseItemProperties<T>(JsonElement itemJson, string itemCategoryId)
            where T : Item, new()
        {
            T item = new()
            {
                CategoryId = itemCategoryId,
                IconLink = itemJson.GetProperty("iconLink").GetString()!,
                Id = itemJson.GetProperty("id").GetString()!,
                ImageLink = itemJson.GetProperty("inspectImageLink").GetString()!,
                MarketLink = itemJson.GetProperty("link").GetString()!,
                Name = itemJson.GetProperty("name").GetString()!,
                ShortName = itemJson.GetProperty("shortName").GetString()!,
                Weight = itemJson.GetProperty("weight").GetDouble(),
                WikiLink = itemJson.GetProperty("wikiLink").GetString()!
            };

            if (TryDeserializeArray(itemJson, "conflictingItems", out ArrayEnumerator conflictingItemsJson))
            {
                List<string> conflictingItemIds = [];

                foreach (JsonElement conflictingItemJson in conflictingItemsJson)
                {
                    conflictingItemIds.Add(conflictingItemJson.GetProperty("id").GetString()!);
                }

                item.ConflictingItemIds = [.. conflictingItemIds];
            }

            ItemMissingProperties? itemForMissingProperties = ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == item.Id);

            if (itemForMissingProperties != null)
            {
                item.MaxStackableAmount = itemForMissingProperties.MaxStackableAmount;
            }

            return item;
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
                BaseItemId = baseItem.Id,
                CategoryId = baseItem.CategoryId,
                ConflictingItemIds = baseItem.ConflictingItemIds,
                IconLink = presetJson.GetProperty("iconLink").GetString()!,
                Id = presetId,
                ImageLink = presetJson.GetProperty("inspectImageLink").GetString()!,
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
        /// Deserilizes a <see cref="Backpack"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Container"/>.</returns>
        private Backpack DeserializeBackpack(JsonElement itemJson, string itemCategoryId)
        {
            Backpack backpack = DeserializeBaseItemProperties<Backpack>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                backpack.Capacity = propertiesJson.GetProperty("capacity").GetDouble();
                backpack.ErgonomicsModifierPercentage = propertiesJson.GetProperty("ergoPenalty").GetDouble();
                backpack.ErgonomicsModifierPercentage = propertiesJson.GetProperty("ergoPenalty").GetDouble();
                backpack.MovementSpeedModifierPercentage = propertiesJson.GetProperty("speedPenalty").GetDouble();
                backpack.TurningSpeedModifierPercentage = propertiesJson.GetProperty("turnPenalty").GetDouble();
            }

            return backpack;
        }

        /// <summary>
        /// Deserilizes a <see cref="Container"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Container"/>.</returns>
        private Container DeserializeContainer(JsonElement itemJson, string itemCategoryId)
        {
            Container container = DeserializeBaseItemProperties<Container>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                container.Capacity = propertiesJson.GetProperty("capacity").GetDouble();
            }

            return container;
        }

        /// <summary>
        /// Deserializes data representing an item into a list of items.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="items">List of items the deserialized item will be stored into.</param>
        private void DeserializeData(DataDeserializationType deserializationType, JsonElement itemJson, ConcurrentBag<Item> items)
        {
            try
            {
                Item? item = null;

                if (deserializationType == DataDeserializationType.Item)
                {
                    item = DeserializeItemData(itemJson);
                }
                else
                {
                    item = DeserializePresetItemData(itemJson, items);
                }

                if (item != null)
                {
                    items.Add(item);
                }
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, e);
                Logger.LogError(error);
            }
        }

        /// <summary>
        /// Deserilizes <see cref="Eyewear"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Eyewear"/>.</returns>
        private Eyewear DeserializeEyewear(JsonElement itemJson, string itemCategoryId)
        {
            Eyewear eyewear = DeserializeArmor<Eyewear>(itemJson, itemCategoryId);

            return eyewear;
        }

        /// <summary>
        /// Deserilizes a <see cref="Grenade"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Grenade"/>.</returns>
        private Grenade DeserializeGrenade(JsonElement itemJson, string itemCategoryId)
        {
            Grenade grenade = DeserializeBaseItemProperties<Grenade>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                grenade.Blinding = propertiesJson.GetProperty("type").GetString() == "Flashbang";
                grenade.ExplosionDelay = propertiesJson.GetProperty("fuse").GetDouble();
                grenade.Impact = propertiesJson.GetProperty("type").GetString() == "Impact Grenade";
                grenade.FragmentsAmount = propertiesJson.GetProperty("fragments").GetDouble();
                grenade.MaximumExplosionRange = propertiesJson.GetProperty("maxExplosionDistance").GetDouble();
                grenade.MinimumExplosionRange = propertiesJson.GetProperty("minExplosionDistance").GetDouble();
                grenade.Smoke = propertiesJson.GetProperty("type").GetString() == "Smoke";

                if (grenade.MaximumExplosionRange == 0)
                {
                    grenade.MaximumExplosionRange = propertiesJson.GetProperty("contusionRadius").GetDouble();
                }

                if (grenade.MinimumExplosionRange == 0)
                {
                    grenade.MinimumExplosionRange = propertiesJson.GetProperty("contusionRadius").GetDouble();
                }
            }

            return grenade;
        }

        /// <summary>
        /// Deserilizes <see cref="Headwear"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Headwear"/>.</returns>
        private Headwear DeserializeHeadwear(JsonElement itemJson, string itemCategoryId)
        {
            Headwear headwear = DeserializeArmor<Headwear>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                if (TryDeserializeBoolean(propertiesJson, "blocksHeadset", out bool blocksHeadphones))
                {
                    headwear.BlocksHeadphones = blocksHeadphones;
                }

                if (TryDeserializeString(propertiesJson, "deafening", out string deafening))
                {
                    headwear.Deafening = deafening;
                }

                ModSlot[] modSlots = DeserializeModSlots(propertiesJson);
                headwear.ModSlots = [.. headwear.ModSlots, .. modSlots];

                if (TryDeserializeDouble(propertiesJson, "ricochetX", out double ricochetChance))
                {
                    headwear.RicochetChance = GetRicochetChance(ricochetChance);
                }
            }

            return headwear;
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
            Headwear presetItem = DeserializeArmorPreset<Headwear>(presetId, presetJson, baseItem);

            presetItem.BlocksHeadphones = baseItem.BlocksHeadphones;
            presetItem.Deafening = baseItem.Deafening;
            presetItem.RicochetChance = baseItem.RicochetChance;

            return presetItem;
        }

        /// <summary>
        /// Deserializes an <see cref="Magazine"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="Magazine"/> preset.</returns>
        private static Magazine DeserializeMagazinePreset(string presetId, JsonElement presetJson, IMagazine baseItem)
        {
            Magazine presetItem = DeserializeBasePresetProperties<Magazine>(presetId, presetJson, baseItem);

            presetItem.AcceptedAmmunitionIds = baseItem.AcceptedAmmunitionIds;
            presetItem.Capacity = baseItem.Capacity;
            presetItem.CheckSpeedModifierPercentage = baseItem.CheckSpeedModifierPercentage;
            presetItem.ErgonomicsModifier = baseItem.ErgonomicsModifier;
            presetItem.LoadSpeedModifierPercentage = baseItem.LoadSpeedModifierPercentage;
            presetItem.MalfunctionPercentage = baseItem.MalfunctionPercentage;

            return presetItem;
        }

        /// <summary>
        /// Deserilizes an <see cref="Item"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Item"/>.</returns>
        private Item DeserializeItem(JsonElement itemJson, string itemCategoryId)
        {
            return DeserializeBaseItemProperties<Item>(itemJson, itemCategoryId);
        }

        /// <summary>
        /// Deserializes the category of an item.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <returns>Deserialized item category.</returns>
        private ItemCategory DeserializeItemCategory(JsonElement itemJson)
        {
            List<string> tarkovItemCategoryIds = [];
            JsonElement tarkovItemCategoriesJson = itemJson.GetProperty("categories");

            foreach (JsonElement tarkovItemCategoryJson in tarkovItemCategoriesJson.EnumerateArray())
            {
                tarkovItemCategoryIds.Add(tarkovItemCategoryJson.GetProperty("id").GetString()!);
            }

            string itemId = itemJson.GetProperty("id").GetString()!;
            ItemCategory itemCategory = GetItemItemCategory(itemId, tarkovItemCategoryIds);

            return itemCategory;
        }

        /// <summary>
        /// Deserializes data representing an item.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <returns>Deserialized item.</returns>
        private Item? DeserializeItemData(JsonElement itemJson)
        {
            if (IsPreset(itemJson))
            {
                return null;
            }

            ItemCategory itemCategory = DeserializeItemCategory(itemJson);

            switch (itemCategory.Id)
            {
                case "ammunition":
                    return DeserializeAmmunition(itemJson, itemCategory.Id);
                case "armor":
                    return DeserializeArmor<Armor>(itemJson, itemCategory.Id);
                case "armorMod":
                    return DeserializeArmorMod(itemJson, itemCategory.Id);
                case "backpack":
                    return DeserializeBackpack(itemJson, itemCategory.Id);
                case "container":
                case "securedContainer":
                    return DeserializeContainer(itemJson, itemCategory.Id);
                case "eyewear":
                    return DeserializeEyewear(itemJson, itemCategory.Id);
                case "grenade":
                    return DeserializeGrenade(itemJson, itemCategory.Id);
                case "faceCover":
                case "headwear":
                    return DeserializeHeadwear(itemJson, itemCategory.Id);
                case "armband":
                case "currency":
                case "headphones":
                case "other":
                case "special":
                    return DeserializeItem(itemJson, itemCategory.Id);
                case "magazine":
                    return DeserializeMagazine(itemJson, itemCategory.Id);
                case "meleeWeapon":
                    return DeserializeMeleeWeapon(itemJson, itemCategory.Id);
                case "mod":
                    return DeserializeMod(itemJson, itemCategory.Id);
                case "mainWeapon":
                case "secondaryWeapon":
                    return DeserializeRangedWeapon(itemJson, itemCategory.Id);
                case "rangedWeaponMod":
                    return DeserializeRangedWeaponMod(itemJson, itemCategory.Id);
                case "vest":
                    return DeserializeVest(itemJson, itemCategory.Id);
                default:
                    throw new NotImplementedException(Properties.Resources.ItemCategoryNotImplemented);
            }
        }

        /// <summary>
        /// Deserilizes a <see cref="Magazine"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Magazine"/>.</returns>
        private Magazine DeserializeMagazine(JsonElement itemJson, string itemCategoryId)
        {
            Magazine magazine = DeserializeBaseItemProperties<Magazine>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                magazine.AcceptedAmmunitionIds = [.. propertiesJson.GetProperty("allowedAmmo")!.EnumerateArray().Select(allowedAmmoJson => allowedAmmoJson.GetProperty("id").GetString()!)];
                magazine.Capacity = propertiesJson.GetProperty("capacity").GetDouble();
                magazine.CheckSpeedModifierPercentage = propertiesJson.GetProperty("ammoCheckModifier").GetDouble();
                magazine.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
                magazine.LoadSpeedModifierPercentage = propertiesJson.GetProperty("loadModifier").GetDouble();
                magazine.MalfunctionPercentage = propertiesJson.GetProperty("malfunctionChance").GetDouble();
                magazine.ModSlots = DeserializeModSlots(propertiesJson);

                if (magazine.ModSlots.Any(ms => ms.Name.StartsWith("camora_")))
                {
                    // Cylinder magazines have no capacity because they have mod slots where ammunition can be placed.
                    // This prevents the "Content" tab from appearing.
                    // We place ammunition in the mod slots instead of the content because some ammunition cannot be stacked
                    // to the amount of the "camora_" modslots (grenades for grenade launcher cannot be stacked)
                    magazine.Capacity = 0;
                }
            }

            return magazine;
        }

        /// <summary>
        /// Deserilizes a <see cref="MeleeWeapon"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="MeleeWeapon"/>.</returns>
        private MeleeWeapon DeserializeMeleeWeapon(JsonElement itemJson, string itemCategoryId)
        {
            MeleeWeapon meleeWeapon = DeserializeBaseItemProperties<MeleeWeapon>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                meleeWeapon.ChopDamage = propertiesJson.GetProperty("slashDamage").GetDouble();
                meleeWeapon.HitRadius = propertiesJson.GetProperty("hitRadius").GetDouble();
                meleeWeapon.StabDamage = propertiesJson.GetProperty("stabDamage").GetDouble();
            }

            return meleeWeapon;
        }

        /// <summary>
        /// Deserilizes a <see cref="Mod"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Mod"/>.</returns>
        private Mod DeserializeMod(JsonElement itemJson, string itemCategoryId)
        {
            Mod mod = DeserializeBaseItemProperties<Mod>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                mod.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
                mod.ModSlots = DeserializeModSlots(propertiesJson);
            }

            return mod;
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
        /// Deserializes an array of <see cref="ModSlot"/>.
        /// </summary>
        /// <param name="propertiesJson">Json element representing the properties of an item.</param>
        /// <returns>Deserialized array of <see cref="ModSlot"/>.</returns>
        private static ModSlot[] DeserializeModSlots(JsonElement propertiesJson)
        {
            List<ModSlot> modSlots = [];

            if (propertiesJson.TryGetProperty("slots", out JsonElement modSlotsJson))
            {
                foreach (JsonElement modSlotJson in modSlotsJson.EnumerateArray())
                {
                    ModSlot modSlot = new()
                    {
                        CompatibleItemIds = [.. modSlotJson.GetProperty("filters").GetProperty("allowedItems").EnumerateArray().Select(ai => ai.GetProperty("id").GetString()!)],
                        Name = modSlotJson.GetProperty("nameId").GetString()!.ToLowerInvariant()
                    };
                    modSlots.Add(modSlot);
                }
            }

            return [.. modSlots];
        }

        /// <summary>
        /// Deserializes data representing an item.
        /// </summary>
        /// <param name="presetItemJson">Json element representing the preset item to deserialize.</param>
        /// <param name="items">Items.</param>
        /// <returns>Deserialized preset item.</returns>
        private Item? DeserializePresetItemData(JsonElement presetItemJson, ConcurrentBag<Item> items)
        {
            if (!IsPreset(presetItemJson)
                || !TryDeserializeObject(presetItemJson, "properties", out JsonElement propertiesJson)
                || propertiesJson.EnumerateObject().Count() <= 1)
            {
                return null;
            }

            string presetId = presetItemJson.GetProperty("id").GetString()!;
            string baseItemId = propertiesJson.GetProperty("baseItem").GetProperty("id").GetString()!;
            Item? baseItem = items.FirstOrDefault(i => i.Id == baseItemId);

            if (baseItem == null)
            {
                throw new InvalidDataException(string.Format(Properties.Resources.ItemNotFound, baseItemId));
            }

            IItem? presetItem = null;

            switch (baseItem)
            {
                case IArmorMod:
                    presetItem = DeserializeArmorModPreset(presetId, presetItemJson, (IArmorMod)baseItem);
                    break;
                case IHeadwear:
                    presetItem = DeserializeHeadwearPreset(presetId, presetItemJson, (IHeadwear)baseItem);
                    break;
                case IVest:
                    presetItem = DeserializeVestPreset(presetId, presetItemJson, (IVest)baseItem);
                    break;
                case IArmor:
                    presetItem = DeserializeArmorPreset<Armor>(presetId, presetItemJson, (IArmor)baseItem);
                    break;
                case IMagazine:
                    presetItem = DeserializeMagazinePreset(presetId, presetItemJson, (IMagazine)baseItem);
                    break;
                case IRangedWeapon:
                    presetItem = DeserializeRangedWeaponPreset(presetId, presetItemJson, propertiesJson, (IRangedWeapon)baseItem);
                    break;
                case IRangedWeaponMod:
                    presetItem = DeserializeRangedWeaponModPreset(presetId, presetItemJson, (IRangedWeaponMod)baseItem);
                    break;
                case IMod:
                    presetItem = DeserializeModPreset(presetId, presetItemJson, (IMod)baseItem);
                    break;
                default:
                    // Special case for items like "customdogtags12345678910" that is a preset of dogtags.
                    // The base item is not moddable, but we need to include it because it can be used in a barter.
                    ItemCategory itemCategory = DeserializeItemCategory(presetItemJson);
                    presetItem = DeserializeItem(presetItemJson, itemCategory.Id);
                    break;
            }

            return (Item)presetItem;
        }

        /// <summary>
        /// Deserilizes a <see cref="RangedWeapon"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="RangedWeapon"/>.</returns>
        private RangedWeapon? DeserializeRangedWeapon(JsonElement itemJson, string itemCategoryId)
        {
            RangedWeapon? rangedWeapon = DeserializeBaseItemProperties<RangedWeapon>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                rangedWeapon.Caliber = propertiesJson.GetProperty("caliber").GetString()!;
                rangedWeapon.Ergonomics = propertiesJson.GetProperty("ergonomics").GetDouble();
                rangedWeapon.FireModes = [.. propertiesJson.GetProperty("fireModes").EnumerateArray().Select(fireModeJson => fireModeJson.GetString()!.ToPascalCase())];
                rangedWeapon.FireRate = propertiesJson.GetProperty("fireRate").GetDouble();
                rangedWeapon.HorizontalRecoil = propertiesJson.GetProperty("recoilHorizontal").GetDouble();
                rangedWeapon.VerticalRecoil = propertiesJson.GetProperty("recoilVertical").GetDouble();

                List<ModSlot> modSlots = [];
                modSlots.AddRange(DeserializeModSlots(propertiesJson));
                rangedWeapon.ModSlots = [.. modSlots];

                if (TryDeserializeObject(propertiesJson, "defaultPreset", out JsonElement defaultPresetJson))
                {
                    rangedWeapon.DefaultPresetId = defaultPresetJson.GetProperty("id").GetString();
                }
            }

            return rangedWeapon;
        }

        /// <summary>
        /// Deserializes a <see cref="RangedWeapon"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="propertiesJson">Json element representing the properties of the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="RangedWeapon"/> preset.</returns>
        private RangedWeapon DeserializeRangedWeaponPreset(string presetId, JsonElement presetJson, JsonElement propertiesJson, IRangedWeapon baseItem)
        {
            RangedWeapon presetItem = DeserializeBasePresetProperties<RangedWeapon>(presetId, presetJson, baseItem);

            presetItem.Caliber = baseItem.Caliber;
            presetItem.Ergonomics = baseItem.Ergonomics;
            presetItem.FireModes = baseItem.FireModes;
            presetItem.FireRate = baseItem.FireRate;
            presetItem.HorizontalRecoil = baseItem.HorizontalRecoil;
            presetItem.VerticalRecoil = baseItem.VerticalRecoil;

            if (TryDeserializeDouble(propertiesJson, "moa", out double moa))
            {
                presetItem.MinuteOfAngle = moa;
            }

            return presetItem;
        }

        /// <summary>
        /// Deserilizes a <see cref="RangedWeaponMod"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="RangedWeaponMod"/>.</returns>
        private RangedWeaponMod DeserializeRangedWeaponMod(JsonElement itemJson, string itemCategoryId)
        {
            RangedWeaponMod rangedWeaponMod = DeserializeBaseItemProperties<RangedWeaponMod>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                if (propertiesJson.TryGetProperty("accuracyModifier", out JsonElement accuracyModifierJson))
                {
                    rangedWeaponMod.AccuracyModifierPercentage = accuracyModifierJson.GetDouble();
                }

                rangedWeaponMod.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
                rangedWeaponMod.ModSlots = DeserializeModSlots(propertiesJson);
                rangedWeaponMod.RecoilModifierPercentage = propertiesJson.GetProperty("recoilModifier").GetDouble();
            }

            return rangedWeaponMod;
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
            presetItem.RecoilModifierPercentage = baseItem.RecoilModifierPercentage;

            return presetItem;
        }

        /// <summary>
        /// Deserilizes a <see cref="Vest"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Vest"/>.</returns>
        private Vest DeserializeVest(JsonElement itemJson, string itemCategoryId)
        {
            Vest vest = DeserializeArmor<Vest>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                vest.Capacity = propertiesJson.GetProperty("capacity").GetDouble();
            }

            return vest;
        }

        /// <summary>
        /// Deserializes an <see cref="Vest"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="Vest"/> preset.</returns>
        private static Vest DeserializeVestPreset(string presetId, JsonElement presetJson, IVest baseItem)
        {
            Vest presetItem = DeserializeArmorPreset<Vest>(presetId, presetJson, baseItem);

            presetItem.Capacity = baseItem.Capacity;

            return presetItem;
        }

        /// <summary>
        /// Gets the name of an armored area from a JSON property.
        /// </summary>
        /// <param name="armoredAreaJson">Armored area from a JSON property</param>
        /// <returns>Armored area name.</returns>
        private static string GetArmoredAreaName(JsonElement armoredAreaJson)
        {
            string armoredArea = armoredAreaJson.GetString()!
                .Replace("F. PLATE", "FR. PLATE") // Some items have "F. PLATE" instead of "FR. PLATE"
                .ToPascalCase()
                .Replace(",", string.Empty)
                .Replace(".", string.Empty);

            return armoredArea;
        }

        /// <summary>
        /// Gets an the item category of an item.
        /// It return the first item category that contains the item in its additional items.
        /// If no item category is found, it searches for the first item category that contains one of the item tarkov item categories.
        /// </summary>
        /// <param name="itemId">Item ID.</param>
        /// <param name="tarkovItemCategoryIds">Tarkov item category IDs.</param>
        /// <returns>Item category.</returns>
        private ItemCategory GetItemItemCategory(string itemId, IEnumerable<string> tarkovItemCategoryIds)
        {
            ItemCategory? result = ItemCategories.FirstOrDefault(ic => ic.AdditionalItemIds.Any(aii => aii == itemId));

            if (result != null)
            {
                return result;
            }

            foreach (string tarkovItemCategoryId in tarkovItemCategoryIds)
            {

                result = ItemCategories.FirstOrDefault(ic => ic.Types.Any(tic => tic.Id == tarkovItemCategoryId));

                if (result != null)
                {
                    return result;
                }
            }

            return new ItemCategory() { Id = "other" };
        }

        /// <summary>
        /// Gets a ricochet chance of an item.
        /// </summary>
        /// <param name="ricochetXValue">Ricochet X value.</param>
        /// <returns>Ricochet chance.</returns>
        private string GetRicochetChance(double ricochetXValue)
        {
            RicochetChance? ricochetChance = TarkovValues.RicochetChances.FirstOrDefault((rc) =>
            {
                // Forced write the condition this way for code coverage
                bool isGreaterThanMin = rc.XMinValue <= ricochetXValue;
                bool isLesserThanMax = rc.XMaxValue >= ricochetXValue;

                return isGreaterThanMin && isLesserThanMax;
            });

            return ricochetChance != null ? ricochetChance.Name.ToPascalCase() : string.Empty;
        }

        /// <summary>
        /// Indicates whether an item is a preset.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <returns><c>true</c> if the item being deserialized is a preset; otherwise <c>false</c>.</returns>
        private bool IsPreset(JsonElement itemJson)
        {
            return TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson)
                && propertiesJson.EnumerateObject().Any()
                && propertiesJson.GetProperty("__typename").GetString() == "ItemPropertiesPreset";
        }

        /// <summary>
        /// Represents the type of elements that should be deserialized.
        /// </summary>
        private enum DataDeserializationType
        {
            /// <summary>
            /// Item deserialization.
            /// </summary>
            Item,

            /// <summary>
            /// Preset deserialization.
            /// </summary>
            Preset
        }
    }
}
