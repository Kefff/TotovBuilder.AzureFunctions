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
using static TotovBuilder.AzureFunctions.Utils.JsonDeserializer;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an items fetcher.
    /// </summary>
    public class ItemsFetcher : ApiFetcher<IEnumerable<Item>>
    {
        /// <summary>
        /// Types of item properties that are read.
        /// Properties of other types are ignored.
        /// </summary>
        private static readonly HashSet<string> ReadPropertiesTypes =
        [
            "ItemPropertiesAmmo",
            "ItemPropertiesArmor",
            "ItemPropertiesArmorAttachment",
            "ItemPropertiesBackpack",
            "ItemPropertiesBarrel",
            "ItemPropertiesChestRig",
            "ItemPropertiesContainer",
            "ItemPropertiesGlasses",
            "ItemPropertiesGrenade",
            "ItemPropertiesHeadwear",
            "ItemPropertiesHelmet",
            "ItemPropertiesMagazine",
            "ItemPropertiesMelee",
            "ItemPropertiesPreset",
            "ItemPropertiesScope",
            "ItemPropertiesWeapon",
            "ItemPropertiesWeaponMod"
        ];

        /// <summary>
        ///  Language in which items are fetched from the API.
        /// </summary>
        public string Language { get; init; }

        /// <inheritdoc/>
        protected override DataType DataType
        {
            get
            {
                return DataType.Items;
            }
        }

        /// <summary>
        /// API requester.
        /// </summary>
        private readonly IApiRequester ApiRequester;

        /// <summary>
        /// Translated armor material names by armor material ID.
        /// </summary>
        private Dictionary<string, string> ArmorMaterialNames = [];

        /// <summary>
        /// Configuration wrapper.
        /// </summary>
        private readonly IConfigurationWrapper ConfigurationWrapper;

        /// <summary>
        /// List of item categories.
        /// </summary>
        private IEnumerable<ItemCategory> ItemCategories = Array.Empty<ItemCategory>();

        /// <summary>
        /// Item categories fetcher.
        /// </summary>
        private readonly IItemCategoriesFetcher ItemCategoriesFetcher;

        /// <summary>
        /// Tarkov values fetcher.
        /// </summary>
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <summary>
        /// Tarkov values.
        /// </summary>
        private TarkovValues TarkovValues = new();

        /// <summary>
        /// Translations of the items texts in the fetched language.
        /// </summary>
        private Translations Translations = new(new JsonElement());

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsFetcher"/> class.
        /// </summary>
        /// <param name="language">Language in which items are fetched.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="apiRequester">API requester.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="itemCategoriesFetcher">Item categories fetcher.</param>
        /// <param name="tarkovValuesFetcher">Tarkov values fetcher.</param>
        public ItemsFetcher(
            string language,
            ILogger<ItemsFetcher> logger,
            IApiRequester apiRequester,
            IConfigurationWrapper configurationWrapper,
            IItemCategoriesFetcher itemCategoriesFetcher,
            ITarkovValuesFetcher tarkovValuesFetcher)
            : base(logger)
        {
            ApiRequester = apiRequester;
            ConfigurationWrapper = configurationWrapper;
            Language = language;
            ItemCategoriesFetcher = itemCategoriesFetcher;
            TarkovValuesFetcher = tarkovValuesFetcher;
        }

        /// <inheritdoc/>
        protected override async Task<Result<IEnumerable<Item>>> ExecuteFetch()
        {
            string itemsPath = BuildApiPath(ConfigurationWrapper.Values.ApiItemsPath, ConfigurationWrapper.Values.ItemsGameMode);
            string translationsPath = BuildApiPath(ConfigurationWrapper.Values.ApiItemsTranslationsPath, ConfigurationWrapper.Values.ItemsGameMode, Language);

            Task<Result<JsonElement>> itemsDataTask = ApiRequester.RequestData(itemsPath);
            Task<Result<Translations>> translationsTask = ApiRequester.RequestTranslations(translationsPath);
            Task<Result> itemCategoriesTask = ItemCategoriesFetcher.Fetch();
            Task<Result> tarkovValuesTask = TarkovValuesFetcher.Fetch();

            await Task.WhenAll(itemsDataTask, translationsTask, itemCategoriesTask, tarkovValuesTask);

            Result allTasksResult = Result.Merge(
                itemsDataTask.Result.ToResult(),
                translationsTask.Result.ToResult(),
                itemCategoriesTask.Result,
                tarkovValuesTask.Result);

            if (allTasksResult.IsFailed)
            {
                return allTasksResult;
            }

            ItemCategories = ItemCategoriesFetcher.FetchedData!;
            TarkovValues = TarkovValuesFetcher.FetchedData!;
            Translations = translationsTask.Result.Value;

            JsonElement itemsDataJson = itemsDataTask.Result.Value;

            Result<JsonElement> itemsResult = ItemsData.GetItems(itemsDataJson, itemsPath, Logger);

            if (itemsResult.IsFailed)
            {
                return itemsResult.ToResult();
            }

            JsonElement itemsJson = itemsResult.Value;

            ArmorMaterialNames = DeserializeArmorMaterialNames(itemsDataJson);

            List<Task> deserializationTasks = [];
            ConcurrentBag<Item> items = [];

            foreach (JsonProperty itemJson in itemsJson.EnumerateObject())
            {
                deserializationTasks.Add(Task.Run(() => DeserializeData(DataDeserializationType.Item, itemJson.Value, items)));
            }

            await Task.WhenAll(deserializationTasks);
            deserializationTasks.Clear();

            foreach (JsonProperty itemJson in itemsJson.EnumerateObject())
            {
                deserializationTasks.Add(Task.Run(() => DeserializeData(DataDeserializationType.Preset, itemJson.Value, items)));
            }

            await Task.WhenAll(deserializationTasks);

            return Result.Ok(items.AsEnumerable());
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
            {
                ammunition.AccuracyModifierPercentage = propertiesJson.GetProperty("accuracyModifier").GetDouble();
                ammunition.ArmorDamagePercentage = propertiesJson.GetProperty("armorDamage").GetDouble() / 100;
                ammunition.Blinding = TryDeserializeString(propertiesJson, "ammoType", out string ammunitionType) && ammunitionType == "flashbang";
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
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

                if (TryDeserializeString(propertiesJson, "material", out string materialId))
                {
                    armor.Material = ArmorMaterialNames.TryGetValue(materialId, out string? materialName) ? materialName : materialId;
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
            }

            return armor;
        }

        /// <summary>
        /// Deserializes the translated names of armor materials.
        /// </summary>
        /// <param name="itemsDataJson">Json element representing the items data.</param>
        /// <returns>Translated armor material names by armor material ID.</returns>
        private Dictionary<string, string> DeserializeArmorMaterialNames(JsonElement itemsDataJson)
        {
            Dictionary<string, string> armorMaterialNames = [];

            if (!TryDeserializeObject(itemsDataJson, "armorMaterials", out JsonElement armorMaterialsJson))
            {
                return armorMaterialNames;
            }

            foreach (JsonProperty armorMaterialJson in armorMaterialsJson.EnumerateObject())
            {
                if (TryDeserializeString(armorMaterialJson.Value, "name", out string name))
                {
                    armorMaterialNames[armorMaterialJson.Name] = Translations.Translate(name);
                }
            }

            return armorMaterialNames;
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
            {
                ModSlot[] modsSlots = DeserializeModSlots(propertiesJson);
                armorMod.ModSlots = [.. armorMod.ModSlots, .. modsSlots];

                if (TryDeserializeArray(propertiesJson, "zones", out ArrayEnumerator headArmoredAreasJson))
                {
                    armorMod.ArmoredAreas = [.. headArmoredAreasJson.Select(headArmoredArea => Translations.Translate(headArmoredArea.GetString()!)).Distinct()];
                }
            }

            return armorMod;
        }

        /// <summary>
        /// Deserializes an <see cref="ArmorMod"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="ArmorMod"/> preset.</returns>
        private ArmorMod DeserializeArmorModPreset(string presetId, JsonElement presetJson, IArmorMod baseItem)
        {
            ArmorMod presetItem = DeserializeArmorPreset<ArmorMod>(presetId, presetJson, baseItem);

            presetItem.BlindnessProtectionPercentage = baseItem.BlindnessProtectionPercentage;

            return presetItem;
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

            foreach (JsonElement armorModSlotJson in armorModSlotsJson)
            {
                // Only open armor slots, in which plates can be inserted, have allowed plates
                if (TryDeserializeArray(armorModSlotJson, "allowedPlates", out ArrayEnumerator allowedPlatesJson))
                {
                    ModSlot armorModSlot = new()
                    {
                        CompatibleItemIds = [.. allowedPlatesJson.Select(ai => ai.GetString()!)],
                        Caption = TryDeserializeString(armorModSlotJson, "name", out string caption) ? Translations.Translate(caption) : null,
                        Name = armorModSlotJson.GetProperty("nameId").GetString()!.ToLowerInvariant()
                    };
                    armorModSlots.Add(armorModSlot);
                }
                else
                {
                    string armorSlotName = armorModSlotJson.GetProperty("nameId").GetString()!;

                    if (armorSlotName == "Soft_armor_front")
                    {
                        item.ArmorClass = armorModSlotJson.GetProperty("class").GetInt32();
                    }

                    lockedArmorSlotsDurability += armorModSlotJson.GetProperty("durability").GetInt32();
                }

                IEnumerable<string> zones = armorModSlotJson.GetProperty("zones").EnumerateArray().Select(zoneJson => Translations.Translate(zoneJson.GetString()!));
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
        /// Deserializes an <see cref="Armor"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="Armor"/> preset.</returns>
        private T DeserializeArmorPreset<T>(string presetId, JsonElement presetJson, IArmor baseItem)
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

            // Armors do not reference their default preset, so it is determined from the presets.
            // The preset flagged as default takes precedence over a preset whose link designates it as default.
            if (TryGetReadProperties(presetJson, out JsonElement propertiesJson)
                && TryDeserializeBoolean(propertiesJson, "default", out bool isDefault)
                && isDefault)
            {
                baseItem.DefaultPresetId = presetItem.Id;
            }
            else if (presetItem.MarketLink.EndsWith("-default") && baseItem.DefaultPresetId == null)
            {
                baseItem.DefaultPresetId = presetItem.Id;
            }

            return presetItem;
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
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
                Name = Translations.Translate(itemJson.GetProperty("name").GetString()!),
                ShortName = Translations.Translate(itemJson.GetProperty("shortName").GetString()!),
                Weight = itemJson.GetProperty("weight").GetDouble(),
                WikiLink = itemJson.GetProperty("wikiLink").GetString()!
            };

            if (TryDeserializeArray(itemJson, "conflictingItems", out ArrayEnumerator conflictingItemsJson))
            {
                List<string> conflictingItemIds = [];

                foreach (JsonElement conflictingItemJson in conflictingItemsJson)
                {
                    conflictingItemIds.Add(conflictingItemJson.GetString()!);
                }

                item.ConflictingItemIds = [.. conflictingItemIds];
            }

            if (TryDeserializeDouble(itemJson, "stackMaxSize", out double maxStackableAmount))
            {
                item.MaxStackableAmount = maxStackableAmount;
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
        private T DeserializeBasePresetProperties<T>(string presetId, JsonElement presetJson, IModdable baseItem)
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
                Name = Translations.Translate(presetJson.GetProperty("name").GetString()!),
                ShortName = Translations.Translate(presetJson.GetProperty("shortName").GetString()!),
                Weight = baseItem.Weight,
                WikiLink = presetJson.GetProperty("wikiLink").GetString()!
            };

            return preset;
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
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
        private Headwear DeserializeHeadwearPreset(string presetId, JsonElement presetJson, IHeadwear baseItem)
        {
            Headwear presetItem = DeserializeArmorPreset<Headwear>(presetId, presetJson, baseItem);

            presetItem.BlocksHeadphones = baseItem.BlocksHeadphones;
            presetItem.Deafening = baseItem.Deafening;
            presetItem.RicochetChance = baseItem.RicochetChance;

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
                tarkovItemCategoryIds.Add(tarkovItemCategoryJson.GetString()!);
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
            if (ItemsData.IsPreset(itemJson))
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
            {
                magazine.AcceptedAmmunitionIds = [.. propertiesJson.GetProperty("allowedAmmo")!.EnumerateArray().Select(allowedAmmoJson => allowedAmmoJson.GetString()!)];
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
        /// Deserializes an <see cref="Magazine"/> preset.
        /// </summary>
        /// <param name="presetId">Preset ID.</param>
        /// <param name="presetJson">JSON element representing the preset.</param>
        /// <param name="baseItem">Base item.</param>
        /// <returns>Deserialized <see cref="Magazine"/> preset.</returns>
        private Magazine DeserializeMagazinePreset(string presetId, JsonElement presetJson, IMagazine baseItem)
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
        /// Deserilizes a <see cref="MeleeWeapon"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="MeleeWeapon"/>.</returns>
        private MeleeWeapon DeserializeMeleeWeapon(JsonElement itemJson, string itemCategoryId)
        {
            MeleeWeapon meleeWeapon = DeserializeBaseItemProperties<MeleeWeapon>(itemJson, itemCategoryId);

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
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
        private Mod DeserializeModPreset(string presetId, JsonElement presetJson, IMod baseItem)
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
        private ModSlot[] DeserializeModSlots(JsonElement propertiesJson)
        {
            List<ModSlot> modSlots = [];

            if (propertiesJson.TryGetProperty("slots", out JsonElement modSlotsJson))
            {
                foreach (JsonElement modSlotJson in modSlotsJson.EnumerateArray())
                {
                    string? caption = modSlotJson.GetProperty("name").GetString();
                    ModSlot modSlot = new()
                    {
                        Caption = caption != null ? Translations.Translate(caption) : null,
                        CompatibleItemIds = [.. modSlotJson.GetProperty("filters").GetProperty("allowedItems").EnumerateArray().Select(ai => ai.GetString()!)],
                        Name = modSlotJson.GetProperty("nameId").GetString()!,
                        Required = TryDeserializeBoolean(modSlotJson, "required", out bool required) && required
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
            if (!ItemsData.IsPreset(presetItemJson)
                || !TryGetReadProperties(presetItemJson, out JsonElement propertiesJson))
            {
                return null;
            }

            string presetId = presetItemJson.GetProperty("id").GetString()!;
            string baseItemId = propertiesJson.GetProperty("baseItem").GetString()!;
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
            {
                rangedWeapon.Caliber = propertiesJson.GetProperty("caliber").GetString()!;
                rangedWeapon.Ergonomics = propertiesJson.GetProperty("ergonomics").GetDouble();
                rangedWeapon.FireModes = [.. propertiesJson.GetProperty("fireModes").EnumerateArray().Select(fireModeJson => Translations.Translate(fireModeJson.GetString()!))];
                rangedWeapon.FireRate = propertiesJson.GetProperty("fireRate").GetDouble();
                rangedWeapon.HorizontalRecoil = propertiesJson.GetProperty("recoilHorizontal").GetDouble();
                rangedWeapon.VerticalRecoil = propertiesJson.GetProperty("recoilVertical").GetDouble();

                List<ModSlot> modSlots = [];
                modSlots.AddRange(DeserializeModSlots(propertiesJson));
                rangedWeapon.ModSlots = [.. modSlots];

                if (TryDeserializeString(propertiesJson, "defaultPreset", out string defaultPresetId))
                {
                    rangedWeapon.DefaultPresetId = defaultPresetId;
                }
            }

            return rangedWeapon;
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

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
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
        private RangedWeaponMod DeserializeRangedWeaponModPreset(string presetId, JsonElement presetJson, IRangedWeaponMod baseItem)
        {
            RangedWeaponMod presetItem = DeserializeBasePresetProperties<RangedWeaponMod>(presetId, presetJson, baseItem);

            presetItem.ErgonomicsModifier = baseItem.ErgonomicsModifier;
            presetItem.RecoilModifierPercentage = baseItem.RecoilModifierPercentage;

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
        /// Deserilizes a <see cref="Vest"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Vest"/>.</returns>
        private Vest DeserializeVest(JsonElement itemJson, string itemCategoryId)
        {
            Vest vest = DeserializeArmor<Vest>(itemJson, itemCategoryId);

            if (TryGetReadProperties(itemJson, out JsonElement propertiesJson))
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
        private Vest DeserializeVestPreset(string presetId, JsonElement presetJson, IVest baseItem)
        {
            Vest presetItem = DeserializeArmorPreset<Vest>(presetId, presetJson, baseItem);

            presetItem.Capacity = baseItem.Capacity;

            return presetItem;
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
        /// Tries to get the properties of an item when their type is read.
        /// </summary>
        /// <param name="itemJson">Json element representing the item.</param>
        /// <param name="propertiesJson">Json element representing the properties when successful.</param>
        /// <returns><c>true</c> when the item has properties of a read type; otherwise <c>false</c>.</returns>
        private static bool TryGetReadProperties(JsonElement itemJson, out JsonElement propertiesJson)
        {
            return TryDeserializeObject(itemJson, "properties", out propertiesJson)
                && TryDeserializeString(propertiesJson, "propertiesType", out string propertiesType)
                && ReadPropertiesTypes.Contains(propertiesType);
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
