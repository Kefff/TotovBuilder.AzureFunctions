using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Represents an items fetcher.
    /// </summary>
    public class ItemsFetcher : ApiFetcher<IEnumerable<Item>>, IItemsFetcher
    {
        private readonly ItemCategoryFinder _itemCategoryFinder;

        /// <inheritdoc/>
        protected override string ApiQueryKey => TotovBuilder.AzureFunctions.ConfigurationReader.ApiItemsQueryKey;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.Items;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="itemCategoryFinder">Item category finder.</param>
        public ItemsFetcher(
            ILogger logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IConfigurationReader configurationReader,
            ICache cache,
            ItemCategoryFinder itemCategoryFinder
        ) : base(logger, httpClientWrapperFactory, configurationReader, cache)
        {
            _itemCategoryFinder = itemCategoryFinder;
        }
        
        /// <inheritdoc/>
        protected override Task<Result<IEnumerable<Item>>> DeserializeData(string responseContent)
        {
            List<Task> deserializationTasks = new List<Task>();
            List<Result<Item>> itemResults = new List<Result<Item>>();
            JsonElement itemsJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement itemJson in itemsJson.EnumerateArray())
            {
                deserializationTasks.Add(DeserializeData(itemJson).ContinueWith(t => itemResults.Add(t.Result)));
            }
            
            return Task.WhenAll(deserializationTasks).ContinueWith((t) => Result.Merge(itemResults.ToArray()));
        }

        /// <summary>
        /// Deserilizes <see cref="Ammunition"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Ammunition"/>.</returns>
        private Result<Item> DeserializeAmmunition(JsonElement itemJson, string itemCategoryId)
        {
            Result<Ammunition> baseItemPropertiesResult = DeserializeBaseItemProperties<Ammunition>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                Ammunition item = baseItemPropertiesResult.Value;
                item.AccuracyPercentageModifier = propertiesJson.GetProperty("accuracy").GetDouble() / 100;
                item.ArmorDamagePercentage = propertiesJson.GetProperty("armorDamage").GetDouble() / 100;
                //item.ArmorPenetrations = ; // TODO : OBTAIN FROM WIKI
                //item.Blinding = ; // TODO : MISSING
                item.Caliber = propertiesJson.GetProperty("caliber").GetString();
                //item.DurabilityBurnPercentageModifier = ; // TODO : MISSING
                item.FleshDamage = propertiesJson.GetProperty("damage").GetDouble();
                item.FragmentationChancePercentage = propertiesJson.GetProperty("fragmentationChance").GetDouble();
                item.HeavyBleedingPercentageChance =  propertiesJson.GetProperty("heavyBleedModifier").GetDouble();
                item.LightBleedingPercentageChance = propertiesJson.GetProperty("lightBleedModifier").GetDouble();
                item.MaxStackableAmount = propertiesJson.GetProperty("stackMaxSize").GetDouble();
                item.PenetrationPower = propertiesJson.GetProperty("penetrationPower").GetDouble();
                item.Projectiles = propertiesJson.GetProperty("projectileCount").GetDouble();
                item.RecoilPercentageModifier = propertiesJson.GetProperty("recoil").GetDouble() / 100;
                item.Tracer = propertiesJson.GetProperty("tracer").GetBoolean();
                item.Velocity = propertiesJson.GetProperty("initialSpeed").GetDouble();

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(Ammunition), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes an <see cref="Armor"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Armor"/>.</returns>
        private Result<Item> DeserializeArmor(JsonElement itemJson, string itemCategoryId)
        {
            Result<Armor> baseItemPropertiesResult = DeserializeBaseItemProperties<Armor>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                Armor item = baseItemPropertiesResult.Value;
                item.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
                item.ArmoredAreas = GetArmoredAreas(propertiesJson);
                item.Durability = propertiesJson.GetProperty("durability").GetDouble();
                item.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
                item.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString();
                item.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
                //item.RicochetChance = ; // TODO : MISSING
                item.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(Armor), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes an <see cref="ArmorMod"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="ArmorMod"/>.</returns>
        private Result<Item> DeserializeArmorMod(JsonElement itemJson, string itemCategoryId)
        {
            Result<ArmorMod> baseItemPropertiesResult = DeserializeBaseItemProperties<ArmorMod>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                ArmorMod item = baseItemPropertiesResult.Value;
                item.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
                item.ArmoredAreas = GetArmoredAreas(propertiesJson);
                item.Durability = propertiesJson.GetProperty("durability").GetDouble();
                item.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
                //item.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString(); // TODO : MISSING
                //item.ModSlots = ; // TODO : MISSING
                item.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
                //item.RicochetChance = ; // TODO : MISSING
                item.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(ArmorMod), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes the properties of an item that are common between item categories.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Item"/>.</returns>
        private Result<T> DeserializeBaseItemProperties<T>(JsonElement itemJson, string itemCategoryId)
            where T : Item, new()
        {
            try
            {
                T item = new T()
                {
                    CategoryId = itemCategoryId,
                    IconLink = itemJson.GetProperty("iconLink").GetString(),
                    Id = itemJson.GetProperty("id").GetString(),
                    ImageLink = itemJson.GetProperty("imageLink").GetString(),
                    MarketLink = itemJson.GetProperty("link").GetString(),
                    MaxStackableAmount = 1,
                    Name = itemJson.GetProperty("name").GetString(),
                    ShortName = itemJson.GetProperty("shortName").GetString(),
                    Weight = itemJson.GetProperty("weight").GetDouble(),
                    WikiLink = itemJson.GetProperty("wikiLink").GetString()
                };

                //item.ConflictingItemIds = ; // TODO : MISSING

                return Result.Ok(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(T), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes a <see cref="Container"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Container"/>.</returns>
        private Result<Item> DeserializeContainer(JsonElement itemJson, string itemCategoryId)
        {
            Result<Container> baseItemPropertiesResult = DeserializeBaseItemProperties<Container>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                Container item = baseItemPropertiesResult.Value;
                item.Capacity = propertiesJson.GetProperty("capacity").GetDouble();

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(Container), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserializes data representing an item.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <returns>Deserialized item.</returns>
        private async Task<Result<Item>> DeserializeData(JsonElement itemJson)
        {
            List<string> tarkovItemCategories = new List<string>();
            JsonElement tarkovItemCategoriesJson = itemJson.GetProperty("categories");

            foreach (JsonElement tarkovItemCategoryJson in tarkovItemCategoriesJson.EnumerateArray())
            {
                tarkovItemCategories.Add(tarkovItemCategoryJson.GetProperty("id").GetString());
            }

            ItemCategory itemCategory = await _itemCategoryFinder.FindFromTarkovCategoryId(tarkovItemCategories.First());

            switch (itemCategory.Id)
            {
                case "ammunition":
                    return DeserializeAmmunition(itemJson, itemCategory.Id);
                case "armband":
                case "currency":
                case "faceCover":
                case "headphones":
                case "other":
                case "special":
                    return DeserializeItem(itemJson, itemCategory.Id);
                case "armor":
                    return DeserializeArmor(itemJson, itemCategory.Id);
                case "armorMod":
                    return DeserializeArmorMod(itemJson, itemCategory.Id);
                case "backpack":
                case "container":
                case "securedContainer":
                    return DeserializeContainer(itemJson, itemCategory.Id);
                case "eyewear":
                    return DeserializeEyewear(itemJson, itemCategory.Id);
                case "grenade":
                    return DeserializeGrenade(itemJson, itemCategory.Id);
                case "headwear":
                    return DeserializeHeadwear(itemJson, itemCategory.Id);
                case "magazine":
                    return DeserializeMagazine(itemJson, itemCategory.Id);
                case "mainWeapon":
                case "secondaryWeapon":
                    return DeserializeRangedWeapon(itemJson, itemCategory.Id);
                case "meleeWeapon":
                    return DeserializeMeleeWeapon(itemJson, itemCategory.Id);
                case "mod":
                    return DeserializeMod(itemJson, itemCategory.Id);
                case "rangedWeaponMod":
                    return DeserializeRangedWeaponMod(itemJson, itemCategory.Id);
                case "vest":
                    return DeserializeVest(itemJson, itemCategory.Id);
                default:
                    string error = Properties.Resources.NotImplementedItemCategory;
                    Logger.LogError(error);

                    return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes <see cref="Eyewear"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Eyewear"/>.</returns>
        private Result<Item> DeserializeEyewear(JsonElement itemJson, string itemCategoryId)
        {
            Result<Eyewear> baseItemPropertiesResult = DeserializeBaseItemProperties<Eyewear>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                Eyewear item = baseItemPropertiesResult.Value;
                item.BlindnessProtectionPercentage = propertiesJson.GetProperty("blindnessProtection").GetDouble();

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(Eyewear), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes a <see cref="Grenade"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Grenade"/>.</returns>
        private Result<Item> DeserializeGrenade(JsonElement itemJson, string itemCategoryId)
        {
            Result<Grenade> baseItemPropertiesResult = DeserializeBaseItemProperties<Grenade>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                Grenade item = baseItemPropertiesResult.Value;
                item.ExplosionDelay = propertiesJson.GetProperty("fuse").GetDouble();
                //item.FragmentAmmunitionId = ; // TOTO : MISSING
                item.FragmentsAmount = propertiesJson.GetProperty("fragments").GetDouble();
                item.MaximumExplosionRange = propertiesJson.GetProperty("maxExplosionDistance").GetDouble();
                item.MinimumExplosionRange = propertiesJson.GetProperty("minExplosionDistance").GetDouble();
                item.Type = propertiesJson.GetProperty("type").GetString();

                if (item.MaximumExplosionRange == 0)
                {
                    item.MaximumExplosionRange = propertiesJson.GetProperty("ContusionDistance").GetDouble();
                }

                if (item.MinimumExplosionRange == 0)
                {
                    item.MinimumExplosionRange = propertiesJson.GetProperty("ContusionDistance").GetDouble();
                }

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(Grenade), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes <see cref="Headwear"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Headwear"/>.</returns>
        private Result<Item> DeserializeHeadwear(JsonElement itemJson, string itemCategoryId)
        {
            Result<Headwear> baseItemPropertiesResult = DeserializeBaseItemProperties<Headwear>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                Headwear item = baseItemPropertiesResult.Value;
                item.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
                item.ArmoredAreas = GetArmoredAreas(propertiesJson);
                item.Deafening = propertiesJson.GetProperty("deafening").GetString();
                item.Durability = propertiesJson.GetProperty("durability").GetDouble();
                item.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
                item.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString();
                //item.ModSlots = ; // TODO : MISSING
                item.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
                //item.RicochetChance = ; // TODO : MISSING
                item.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(Headwear), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes an <see cref="Item"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Item"/>.</returns>
        private Result<Item> DeserializeItem(JsonElement itemJson, string itemCategoryId)
        {
            return DeserializeBaseItemProperties<Item>(itemJson, itemCategoryId);
        }

        /// <summary>
        /// Deserilizes a <see cref="Magazine"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Magazine"/>.</returns>
        private Result<Item> DeserializeMagazine(JsonElement itemJson, string itemCategoryId)
        {
            Result<Magazine> baseItemPropertiesResult = DeserializeBaseItemProperties<Magazine>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                Magazine item = baseItemPropertiesResult.Value;
                //item.AcceptedAmmunitionIds = ; // TODO : MISSING
                item.Capacity = propertiesJson.GetProperty("capacity").GetDouble();
                item.CheckSpeedPercentageModifier = propertiesJson.GetProperty("ammoCheckModifier").GetDouble();
                item.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
                item.LoadSpeedPercentageModifier = propertiesJson.GetProperty("loadModifier").GetDouble();
                item.MalfunctionPercentage = propertiesJson.GetProperty("malfunctionChance").GetDouble();
                //item.ModSlots = ; // TODO : MISSING

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(Magazine), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes a <see cref="MeleeWeapon"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="MeleeWeapon"/>.</returns>
        private Result<Item> DeserializeMeleeWeapon(JsonElement itemJson, string itemCategoryId)
        {
            Result<MeleeWeapon> baseItemPropertiesResult = DeserializeBaseItemProperties<MeleeWeapon>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                MeleeWeapon item = baseItemPropertiesResult.Value;
                //item.ChopDamage = ; // TODO : MISSING
                //item.HitRadius = ; // TODO : MISSING
                //item.StabDamage = ; // TODO : MISSING

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(MeleeWeapon), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes a <see cref="Mod"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Mod"/>.</returns>
        private Result<Item> DeserializeMod(JsonElement itemJson, string itemCategoryId)
        {
            Result<Mod> baseItemPropertiesResult = DeserializeBaseItemProperties<Mod>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                Mod item = baseItemPropertiesResult.Value;
                item.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
                //item.ModSlots = ; // TODO : MISSING

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(Mod), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes a <see cref="RangedWeapon"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="RangedWeapon"/>.</returns>
        private Result<Item> DeserializeRangedWeapon(JsonElement itemJson, string itemCategoryId)
        {
            Result<RangedWeapon> baseItemPropertiesResult = DeserializeBaseItemProperties<RangedWeapon>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                RangedWeapon item = baseItemPropertiesResult.Value;
                item.Caliber = propertiesJson.GetProperty("caliber").GetString();
                item.Ergonomics = propertiesJson.GetProperty("ergonomics").GetDouble();
                item.FireModes = propertiesJson.GetProperty("fireModes").EnumerateArray().Select((JsonElement fireModeJson) => fireModeJson.GetString().ToStringCase()).ToArray();
                item.FireRate = propertiesJson.GetProperty("fireRate").GetDouble();
                item.HorizontalRecoil = propertiesJson.GetProperty("recoilHorizontal").GetDouble();
                //item.ModSlots = ; // TODO : MISSING
                item.VerticalRecoil = propertiesJson.GetProperty("recoilVertical").GetDouble();

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(RangedWeapon), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes a <see cref="RangedWeaponMod"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="RangedWeaponMod"/>.</returns>
        private Result<Item> DeserializeRangedWeaponMod(JsonElement itemJson, string itemCategoryId)
        {
            Result<RangedWeaponMod> baseItemPropertiesResult = DeserializeBaseItemProperties<RangedWeaponMod>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                RangedWeaponMod item = baseItemPropertiesResult.Value;
                //item.AccuracyPercentageModifier = ; // TODO : MISSING
                item.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
                //item.ModSlots = ; // TODO : MISSING
                item.RecoilPercentageModifier = propertiesJson.GetProperty("recoil").GetDouble() / 100;

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(RangedWeaponMod), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Deserilizes a <see cref="Vest"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Vest"/>.</returns>
        private Result<Item> DeserializeVest(JsonElement itemJson, string itemCategoryId)
        {
            Result<Vest> baseItemPropertiesResult = DeserializeBaseItemProperties<Vest>(itemJson, itemCategoryId);

            if (!baseItemPropertiesResult.IsSuccess)
            {
                return baseItemPropertiesResult.ToResult<Item>();
            }

            try
            {
                JsonElement propertiesJson = itemJson.GetProperty("properties");

                Vest item = baseItemPropertiesResult.Value;
                item.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
                item.ArmoredAreas = GetArmoredAreas(propertiesJson);
                item.Capacity = propertiesJson.GetProperty("capacity").GetDouble();
                item.Durability = propertiesJson.GetProperty("durability").GetDouble();
                item.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
                item.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString();
                item.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
                //item.RicochetChance = ; // TODO : MISSING
                item.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();

                return Result.Ok<Item>(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, typeof(Vest), e);
                Logger.LogError(error);

                return Result.Fail(error);
            }
        }

        /// <summary>
        /// Gets an armored areas.
        /// </summary>
        /// <param name="propertiesJson">Json element representing the properties of an item.</param>
        /// <returns>Armored areas.</returns>
        private string[] GetArmoredAreas(JsonElement propertiesJson)
        {
            List<string> armoredAreas = new List<string>();

            if (propertiesJson.TryGetProperty("zones", out JsonElement armoredAreasJson))
            {
                armoredAreas.AddRange(armoredAreasJson.EnumerateArray().Select((JsonElement armoredAreaJson) => armoredAreaJson.GetString().ToStringCase()));
            }

            if (propertiesJson.TryGetProperty("headZones", out JsonElement headArmoredAreasJson))
            {
                armoredAreas.AddRange(headArmoredAreasJson.EnumerateArray().Select((JsonElement armoredAreaJson) => armoredAreaJson.GetString().ToStringCase()));
            }

            return armoredAreas.ToArray();
        }
    }
}
