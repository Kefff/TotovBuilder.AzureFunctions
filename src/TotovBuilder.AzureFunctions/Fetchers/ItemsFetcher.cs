using System.Collections.Concurrent;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
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
        /// List of armor penetrations.
        /// </summary>
        private IEnumerable<ArmorPenetration> ArmorPenetrations = Array.Empty<ArmorPenetration>();

        /// <summary>
        /// Armor penetrations fetcher.
        /// </summary>
        private readonly IArmorPenetrationsFetcher ArmorPenetrationsFetcher;

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
        /// Tarkov values.
        /// </summary>
        private TarkovValues TarkovValues = new TarkovValues();

        /// <summary>
        /// Tarkov values fetcher.
        /// </summary>
        private readonly ITarkovValuesFetcher TarkovValuesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationWrapper">Configuration wrapper.</param>
        /// <param name="itemCategoriesFetcher">Item categories fetcher.</param>
        /// <param name="itemMissingPropertiesFetcher">Item missing properties fetcher.</param>
        /// <param name="armorPenetrationsFetcher">Armor penetrations fetcher.</param>
        /// <param name="tarkovValuesFetcher">Tarkov values fetcher.</param>
        public ItemsFetcher(
            ILogger<ItemsFetcher> logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IConfigurationWrapper configurationWrapper,
            IItemCategoriesFetcher itemCategoriesFetcher,
            IItemMissingPropertiesFetcher itemMissingPropertiesFetcher,
            IArmorPenetrationsFetcher armorPenetrationsFetcher,
            ITarkovValuesFetcher tarkovValuesFetcher
        ) : base(logger, httpClientWrapperFactory, configurationWrapper)
        {
            ArmorPenetrationsFetcher = armorPenetrationsFetcher;
            ItemCategoriesFetcher = itemCategoriesFetcher;
            ItemMissingPropertiesFetcher = itemMissingPropertiesFetcher;
            TarkovValuesFetcher = tarkovValuesFetcher;
        }

        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<Item>>> DeserializeData(string responseContent)
        {
            List<Task> deserializationTasks = new List<Task>();
            ConcurrentBag<Item> items = new ConcurrentBag<Item>();

            Result<IEnumerable<ArmorPenetration>>? armorPenetrationsResult = null;
            Result<IEnumerable<ItemCategory>>? itemCategoriesResult = null;
            Result<IEnumerable<ItemMissingProperties>>? itemMissingPropertiesResult = null;
            Result<TarkovValues>? tarkovValuesResult = null;

            Task.WaitAll(
                ArmorPenetrationsFetcher.Fetch().ContinueWith(r => armorPenetrationsResult = r.Result),
                ItemCategoriesFetcher.Fetch().ContinueWith(r => itemCategoriesResult = r.Result),
                ItemMissingPropertiesFetcher.Fetch().ContinueWith(r => itemMissingPropertiesResult = r.Result),
                TarkovValuesFetcher.Fetch().ContinueWith(r => tarkovValuesResult = r.Result));
            Result allTasksResult = Result.Merge(armorPenetrationsResult, itemCategoriesResult, itemMissingPropertiesResult, tarkovValuesResult);

            if (allTasksResult.IsSuccess)
            {
                ArmorPenetrations = armorPenetrationsResult!.Value;
                ItemCategories = itemCategoriesResult!.Value;
                ItemMissingProperties = itemMissingPropertiesResult!.Value;
                TarkovValues = tarkovValuesResult!.Value;
            }
            else
            {
                return Task.FromResult(allTasksResult.ToResult<IEnumerable<Item>>());
            }

            JsonElement itemsJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement itemJson in itemsJson.EnumerateArray())
            {
                deserializationTasks.Add(Task.Run(() => DeserializeData(DataDeserializationType.Item, itemJson, items)));
            }

            Task.WaitAll(deserializationTasks.ToArray());
            deserializationTasks.Clear();

            foreach (JsonElement itemJson in itemsJson.EnumerateArray())
            {
                deserializationTasks.Add(Task.Run(() => DeserializeData(DataDeserializationType.Preset, itemJson, items)));
            }

            Task.WaitAll(deserializationTasks.ToArray());

            return Task.FromResult(Result.Ok(items.AsEnumerable()));
        }

        /// <summary>
        /// Deserilizes <see cref="Ammunition"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Ammunition"/>.</returns>
        private Item DeserializeAmmunition(JsonElement itemJson, string itemCategoryId)
        {
            Ammunition ammunition = DeserializeBaseItemProperties<Ammunition>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                ammunition.AccuracyPercentageModifier = propertiesJson.GetProperty("accuracyModifier").GetDouble();
                ammunition.ArmorDamagePercentage = propertiesJson.GetProperty("armorDamage").GetDouble() / 100;
                ammunition.ArmorPenetrations = ArmorPenetrations.FirstOrDefault(ac => ac.AmmunitionId == ammunition.Id)?.Values ?? Array.Empty<double>(); // TODO : OBTAIN FROM WIKI
                //ammunition.Blinding = ; // TODO : MISSING FROM API
                ammunition.Caliber = propertiesJson.GetProperty("caliber").GetString()!;
                ammunition.DurabilityBurnPercentageModifier = Math.Round(propertiesJson.GetProperty("durabilityBurnFactor").GetDouble() - 1, 2);
                ammunition.FleshDamage = propertiesJson.GetProperty("damage").GetDouble();
                ammunition.FragmentationChancePercentage = propertiesJson.GetProperty("fragmentationChance").GetDouble();
                ammunition.HeavyBleedingPercentageChance = propertiesJson.GetProperty("heavyBleedModifier").GetDouble();
                ammunition.LightBleedingPercentageChance = propertiesJson.GetProperty("lightBleedModifier").GetDouble();
                ammunition.MaxStackableAmount = propertiesJson.GetProperty("stackMaxSize").GetDouble();
                ammunition.PenetrationPower = propertiesJson.GetProperty("penetrationPower").GetDouble();
                ammunition.Projectiles = propertiesJson.GetProperty("projectileCount").GetDouble();
                ammunition.RecoilPercentageModifier = propertiesJson.GetProperty("recoilModifier").GetDouble();
                ammunition.Tracer = propertiesJson.GetProperty("tracer").GetBoolean();
                ammunition.Velocity = propertiesJson.GetProperty("initialSpeed").GetDouble();

                ammunition.Subsonic = ammunition.Velocity <= 340;

                if (ammunition.RecoilPercentageModifier > 1)
                {
                    ammunition.RecoilPercentageModifier = Math.Round(ammunition.RecoilPercentageModifier - 1, 2); // Only usefull for 12/70 8.5mm Magnum buckshot which has a recoil modifier of 1.15 instead or 0.15
                }
            }

            return ammunition;
        }

        /// <summary>
        /// Deserilizes an <see cref="Armor"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Armor"/>.</returns>
        private Item DeserializeArmor(JsonElement itemJson, string itemCategoryId)
        {
            Armor armor = DeserializeBaseItemProperties<Armor>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                armor.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
                armor.ArmoredAreas = GetArmoredAreas(propertiesJson);
                armor.Durability = propertiesJson.GetProperty("durability").GetDouble();
                armor.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
                armor.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString()!.ToPascalCase();
                armor.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
                //armor.RicochetChance = ; // TODO : MISSING FROM API
                armor.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();
            }

            return armor;
        }

        /// <summary>
        /// Deserilizes an <see cref="ArmorMod"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="ArmorMod"/>.</returns>
        private Item DeserializeArmorMod(JsonElement itemJson, string itemCategoryId)
        {
            ArmorMod armorMod = DeserializeBaseItemProperties<ArmorMod>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                armorMod.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
                armorMod.ArmoredAreas = GetArmoredAreas(propertiesJson);
                armorMod.BlindnessProtectionPercentage = propertiesJson.GetProperty("blindnessProtection").GetDouble();
                armorMod.Durability = propertiesJson.GetProperty("durability").GetDouble();
                armorMod.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
                armorMod.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString()!.ToPascalCase();
                armorMod.ModSlots = DeserializeModSlots(propertiesJson); // TODO : MISSING FROM API
                armorMod.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
                //item.RicochetChance = ; // TODO : MISSING FROM API
                armorMod.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();
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
        /// Deserilizes the properties of an item that are common between item categories.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Item"/>.</returns>
        private T DeserializeBaseItemProperties<T>(JsonElement itemJson, string itemCategoryId)
            where T : Item, new()
        {
            T item = new T()
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

            ItemMissingProperties? itemForMissingProperties = ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == item.Id);

            if (itemForMissingProperties != null)
            {
                item.ConflictingItemIds = itemForMissingProperties.ConflictingItemIds;
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
            T preset = new T()
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
        private Item DeserializeBackpack(JsonElement itemJson, string itemCategoryId)
        {
            Backpack backpack = DeserializeBaseItemProperties<Backpack>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                backpack.Capacity = propertiesJson.GetProperty("capacity").GetDouble();
                backpack.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
                backpack.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
                backpack.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();
            }

            return backpack;
        }

        /// <summary>
        /// Deserilizes a <see cref="Container"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Container"/>.</returns>
        private Item DeserializeContainer(JsonElement itemJson, string itemCategoryId)
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
        private Item DeserializeEyewear(JsonElement itemJson, string itemCategoryId)
        {
            Eyewear eyewear = DeserializeBaseItemProperties<Eyewear>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                eyewear.BlindnessProtectionPercentage = propertiesJson.GetProperty("blindnessProtection").GetDouble();
            }

            return eyewear;
        }

        /// <summary>
        /// Deserilizes a <see cref="Grenade"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Grenade"/>.</returns>
        private Item DeserializeGrenade(JsonElement itemJson, string itemCategoryId)
        {
            Grenade grenade = DeserializeBaseItemProperties<Grenade>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                grenade.ExplosionDelay = propertiesJson.GetProperty("fuse").GetDouble();
                grenade.FragmentsAmount = propertiesJson.GetProperty("fragments").GetDouble();
                grenade.MaximumExplosionRange = propertiesJson.GetProperty("maxExplosionDistance").GetDouble();
                grenade.MinimumExplosionRange = propertiesJson.GetProperty("minExplosionDistance").GetDouble();
                grenade.Type = propertiesJson.GetProperty("type").GetString()!;

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
        private Item DeserializeHeadwear(JsonElement itemJson, string itemCategoryId)
        {
            Headwear headwear = DeserializeBaseItemProperties<Headwear>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                headwear.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
                headwear.ArmoredAreas = GetArmoredAreas(propertiesJson);
                headwear.BlocksHeadphones = propertiesJson.GetProperty("blocksHeadset").GetBoolean();
                headwear.Deafening = propertiesJson.GetProperty("deafening").GetString()!;
                headwear.Durability = propertiesJson.GetProperty("durability").GetDouble();
                headwear.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
                headwear.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString()!.ToPascalCase();
                headwear.ModSlots = DeserializeModSlots(propertiesJson);
                headwear.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
                headwear.RicochetChance = GetRicochetChance(propertiesJson.GetProperty("ricochetX").GetDouble());
                headwear.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();
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
            presetItem.CheckSpeedPercentageModifier = baseItem.CheckSpeedPercentageModifier;
            presetItem.ErgonomicsModifier = baseItem.ErgonomicsModifier;
            presetItem.LoadSpeedPercentageModifier = baseItem.LoadSpeedPercentageModifier;
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
            List<string> tarkovItemCategoryIds = new List<string>();
            JsonElement tarkovItemCategoriesJson = itemJson.GetProperty("categories");

            foreach (JsonElement tarkovItemCategoryJson in tarkovItemCategoriesJson.EnumerateArray())
            {
                tarkovItemCategoryIds.Add(tarkovItemCategoryJson.GetProperty("id").GetString()!);
            }

            ItemCategory itemCategory = GetItemCategoryFromTarkovCategoryId(tarkovItemCategoryIds);

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
                    return DeserializeArmor(itemJson, itemCategory.Id);
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
                case "headwear":
                    return DeserializeHeadwear(itemJson, itemCategory.Id);
                case "armband":
                case "currency":
                case "faceCover":
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
        private Item DeserializeMagazine(JsonElement itemJson, string itemCategoryId)
        {
            Magazine magazine = DeserializeBaseItemProperties<Magazine>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                magazine.AcceptedAmmunitionIds = propertiesJson.GetProperty("allowedAmmo")!.EnumerateArray().Select(allowedAmmoJson => allowedAmmoJson.GetProperty("id").GetString()!).ToArray();
                magazine.Capacity = propertiesJson.GetProperty("capacity").GetDouble();
                magazine.CheckSpeedPercentageModifier = propertiesJson.GetProperty("ammoCheckModifier").GetDouble();
                magazine.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
                magazine.LoadSpeedPercentageModifier = propertiesJson.GetProperty("loadModifier").GetDouble();
                magazine.MalfunctionPercentage = propertiesJson.GetProperty("malfunctionChance").GetDouble();
                magazine.ModSlots = DeserializeModSlots(propertiesJson);
            }

            return magazine;
        }

        /// <summary>
        /// Deserilizes a <see cref="MeleeWeapon"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="MeleeWeapon"/>.</returns>
        private Item DeserializeMeleeWeapon(JsonElement itemJson, string itemCategoryId)
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
        private Item DeserializeMod(JsonElement itemJson, string itemCategoryId)
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
            List<ModSlot> modSlots = new List<ModSlot>();

            if (propertiesJson.TryGetProperty("slots", out JsonElement modSlotsJson))
            {
                foreach (JsonElement modSlotJson in modSlotsJson.EnumerateArray())
                {
                    ModSlot modSlot = new ModSlot()
                    {
                        CompatibleItemIds = modSlotJson.GetProperty("filters").GetProperty("allowedItems").EnumerateArray().Select(ai => ai.GetProperty("id").GetString()!).ToArray(),
                        Name = modSlotJson.GetProperty("nameId").GetString()!
                    };
                    modSlots.Add(modSlot);
                }
            }

            return modSlots.ToArray();
        }

        /// <summary>
        /// Deserializes data representing an item.
        /// </summary>
        /// <param name="presetItemJson">Json element representing the preset item to deserialize.</param>
        /// <param name="items">Items.</param>
        /// <returns>Deserialized preset item.</returns>
        private Item? DeserializePresetItemData(JsonElement presetItemJson, ConcurrentBag<Item> items)
        {
            if (presetItemJson.GetProperty("name").GetString()! == "Gas tube + handguard")
            {
            }

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
        private Item? DeserializeRangedWeapon(JsonElement itemJson, string itemCategoryId)
        {
            RangedWeapon? rangedWeapon = DeserializeBaseItemProperties<RangedWeapon>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                rangedWeapon.Caliber = propertiesJson.GetProperty("caliber").GetString()!;
                rangedWeapon.Ergonomics = propertiesJson.GetProperty("ergonomics").GetDouble();
                rangedWeapon.FireModes = propertiesJson.GetProperty("fireModes").EnumerateArray().Select((JsonElement fireModeJson) => fireModeJson.GetString()!.ToPascalCase()).ToArray();
                rangedWeapon.FireRate = propertiesJson.GetProperty("fireRate").GetDouble();
                rangedWeapon.HorizontalRecoil = propertiesJson.GetProperty("recoilHorizontal").GetDouble();
                rangedWeapon.VerticalRecoil = propertiesJson.GetProperty("recoilVertical").GetDouble();

                List<ModSlot> modSlots = new List<ModSlot>();
                modSlots.AddRange(ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == rangedWeapon.Id)?.RangedWeaponChambers ?? Array.Empty<ModSlot>());
                modSlots.AddRange(DeserializeModSlots(propertiesJson));
                rangedWeapon.ModSlots = modSlots.ToArray();

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
        private Item DeserializeRangedWeaponMod(JsonElement itemJson, string itemCategoryId)
        {
            RangedWeaponMod rangedWeaponMod = DeserializeBaseItemProperties<RangedWeaponMod>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                if (propertiesJson.TryGetProperty("accuracyModifier", out JsonElement accuracyModifierJson))
                {
                    rangedWeaponMod.AccuracyPercentageModifier = accuracyModifierJson.GetDouble();
                }

                rangedWeaponMod.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
                rangedWeaponMod.ModSlots = DeserializeModSlots(propertiesJson);
                rangedWeaponMod.RecoilPercentageModifier = propertiesJson.GetProperty("recoilModifier").GetDouble();
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
            presetItem.RecoilPercentageModifier = baseItem.RecoilPercentageModifier;

            return presetItem;
        }

        /// <summary>
        /// Deserilizes a <see cref="Vest"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Vest"/>.</returns>
        private Item DeserializeVest(JsonElement itemJson, string itemCategoryId)
        {
            Vest vest = DeserializeBaseItemProperties<Vest>(itemJson, itemCategoryId);

            if (TryDeserializeObject(itemJson, "properties", out JsonElement propertiesJson) && propertiesJson.EnumerateObject().Count() > 1)
            {
                if (TryDeserializeDouble(propertiesJson, "class", out double armorClass))
                {
                    vest.ArmorClass = armorClass;
                }

                vest.ArmoredAreas = GetArmoredAreas(propertiesJson);
                vest.Capacity = propertiesJson.GetProperty("capacity").GetDouble();

                if (TryDeserializeDouble(propertiesJson, "durability", out double durability))
                {
                    vest.Durability = durability;
                }

                if (TryDeserializeDouble(propertiesJson, "ergoPenalty", out double ergonomicsPercentageModifier))
                {
                    vest.ErgonomicsPercentageModifier = ergonomicsPercentageModifier / 100;
                }

                if (TryDeserializeString(propertiesJson.GetProperty("material"), "name", out string material))
                {
                    vest.Material = material.ToPascalCase();
                }

                if (TryDeserializeDouble(propertiesJson, "speedPenalty", out double movementSpeedPercentageModifier))
                {
                    vest.MovementSpeedPercentageModifier = movementSpeedPercentageModifier;
                }

                //vest.RicochetChance = ; // TODO : MISSING FROM API

                if (TryDeserializeDouble(propertiesJson, "turnPenalty", out double turningSpeedPercentageModifier))
                {
                    vest.TurningSpeedPercentageModifier = turningSpeedPercentageModifier;
                }
            }

            return vest;
        }

        /// <summary>
        /// Gets an armored areas.
        /// </summary>
        /// <param name="propertiesJson">Json element representing the properties of an item.</param>
        /// <returns>Armored areas.</returns>
        private string[] GetArmoredAreas(JsonElement propertiesJson)
        {
            List<string> armoredAreas = new List<string>();

            if (TryDeserializeArray(propertiesJson, "zones", out ArrayEnumerator armoredAreasJson))
            {
                foreach (JsonElement armoredAreaJson in armoredAreasJson)
                {
                    if (armoredAreaJson.ValueKind == JsonValueKind.String)
                    {
                        armoredAreas.Add(armoredAreaJson.GetString()!.ToPascalCase());
                    }
                }
            }

            if (TryDeserializeArray(propertiesJson, "headZones", out ArrayEnumerator headArmoredAreasJson))
            {
                foreach (JsonElement headArmoredAreaJson in headArmoredAreasJson)
                {
                    if (headArmoredAreaJson.ValueKind == JsonValueKind.String)
                    {
                        armoredAreas.Add(headArmoredAreaJson.GetString()!.ToPascalCase());
                    }
                }
            }

            return armoredAreas.ToArray();
        }

        /// <summary>
        /// Gets an item category from a tarkov item category ID.
        /// When no matching item category is found, return the "other" item category.
        /// </summary>
        /// <param name="tarkovItemCategoryIds">Tarkov item category IDs.</param>
        /// <returns>Item category.</returns>
        private ItemCategory GetItemCategoryFromTarkovCategoryId(IEnumerable<string> tarkovItemCategoryIds)
        {
            ItemCategory? result;

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
                // Forced to do this for code coverage
                bool isGreaterThanMin = rc.XMinValue <= ricochetXValue;
                bool isLesserThanMax = rc.XMaxValue >= ricochetXValue;

                return isGreaterThanMin && isLesserThanMax;
            });

            return (ricochetChance?.Name ?? string.Empty).ToPascalCase();
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
