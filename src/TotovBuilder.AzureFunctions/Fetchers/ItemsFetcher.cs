using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model;
using TotovBuilder.Model.Items;

namespace TotovBuilder.AzureFunctions.Fetchers
{
    /// <summary>
    /// Represents an items fetcher.
    /// </summary>
    public class ItemsFetcher : ApiFetcher<IEnumerable<Item>>, IItemsFetcher
    {
        /// <inheritdoc/>
        protected override string ApiQuery => AzureFunctionsConfigurationReader.Values.ApiItemsQuery;

        /// <inheritdoc/>
        protected override DataType DataType => DataType.Items;

        /// <summary>
        /// Items category finder.
        /// </summary>
        private readonly ItemCategoryFinder ItemCategoryFinder;

        /// <summary>
        /// List of item missing properties to complete fetched items with.
        /// </summary>
        private IEnumerable<ItemMissingProperties> ItemMissingProperties = Array.Empty<ItemMissingProperties>();

        /// <summary>
        /// Item missing properties fetcher.
        /// </summary>
        private readonly IItemMissingPropertiesFetcher ItemMissingPropertiesFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsFetcher"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="httpClientWrapperFactory">HTTP client wrapper factory.</param>
        /// <param name="azureFunctionsConfigurationReader">Azure Functions configuration reader.</param>
        /// <param name="cache">Cache.</param>
        /// <param name="itemCategoryFinder">Item category finder.</param>
        /// <param name="itemMissingPropertiesFetcher">Item missing properties fetcher.</param>
        public ItemsFetcher(
            ILogger logger,
            IHttpClientWrapperFactory httpClientWrapperFactory,
            IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader,
            ICache cache,
            ItemCategoryFinder itemCategoryFinder,
            IItemMissingPropertiesFetcher itemMissingPropertiesFetcher
        ) : base(logger, httpClientWrapperFactory, azureFunctionsConfigurationReader, cache)
        {
            ItemCategoryFinder = itemCategoryFinder;
            ItemMissingPropertiesFetcher = itemMissingPropertiesFetcher;
        }
        
        /// <inheritdoc/>
        protected override async Task<Result<IEnumerable<Item>>> DeserializeData(string responseContent)
        {
            List<Task> deserializationTasks = new List<Task>();
            List<Item> items = new List<Item>();
            ItemMissingProperties = await ItemMissingPropertiesFetcher.Fetch() ?? Array.Empty<ItemMissingProperties>();

            JsonElement itemsJson = JsonDocument.Parse(responseContent).RootElement;

            foreach (JsonElement itemJson in itemsJson.EnumerateArray())
            {
                deserializationTasks.Add(DeserializeData(itemJson, items));
            }
            
            return await Task.WhenAll(deserializationTasks).ContinueWith((t) => Result.Ok(items.AsEnumerable()));
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

            JsonElement propertiesJson = itemJson.GetProperty("properties");
            ammunition.AccuracyPercentageModifier = propertiesJson.GetProperty("accuracyModifier").GetDouble();
            ammunition.ArmorDamagePercentage = propertiesJson.GetProperty("armorDamage").GetDouble() / 100;
            //ammunition.ArmorPenetrations = ; // TODO : OBTAIN FROM WIKI
            //ammunition.Blinding = ; // TODO : MISSING FROM API
            ammunition.Caliber = propertiesJson.GetProperty("caliber").GetString();
            ammunition.DurabilityBurnPercentageModifier = propertiesJson.GetProperty("durabilityBurnFactor").GetDouble() - 1;
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

            JsonElement propertiesJson = itemJson.GetProperty("properties");
            armor.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
            armor.ArmoredAreas = GetArmoredAreas(propertiesJson);
            armor.Durability = propertiesJson.GetProperty("durability").GetDouble();
            armor.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
            armor.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString();
            armor.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
            //armor.RicochetChance = ; // TODO : MISSING FROM API
            armor.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();

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
            
            JsonElement propertiesJson = itemJson.GetProperty("properties");
            armorMod.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
            armorMod.ArmoredAreas = GetArmoredAreas(propertiesJson);
            armorMod.BlindnessProtectionPercentage = propertiesJson.GetProperty("blindnessProtection").GetDouble();
            armorMod.Durability = propertiesJson.GetProperty("durability").GetDouble();
            armorMod.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
            armorMod.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString();
            armorMod.ModSlots = ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == armorMod.Id)?.ModSlots ?? Array.Empty<ModSlot>();
            armorMod.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
            //item.RicochetChance = ; // TODO : MISSING FROM API
            armorMod.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();

            return armorMod;
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

            ItemMissingProperties? itemForMissingProperties = ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == item.Id);

            if (itemForMissingProperties != null)
            {
                item.ConflictingItemIds = itemForMissingProperties.ConflictingItemIds;
                item.MaxStackableAmount = itemForMissingProperties.MaxStackableAmount;
            }

            return item;
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

            JsonElement propertiesJson = itemJson.GetProperty("properties");

            if (propertiesJson.ValueKind != JsonValueKind.Null)
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
        private async Task DeserializeData(JsonElement itemJson, List<Item> items)
        {
            try
            {
                Item item = await DeserializeData(itemJson);
                items.Add(item);
            }
            catch (Exception e)
            {
                string error = string.Format(Properties.Resources.ItemDeserializationError, e);
                Logger.LogError(error);
            }
        }

        /// <summary>
        /// Deserializes data representing an item.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <returns>Deserialized item.</returns>
        private async Task<Item> DeserializeData(JsonElement itemJson)
        {
            List<string> tarkovItemCategories = new List<string>();
            JsonElement tarkovItemCategoriesJson = itemJson.GetProperty("categories");

            foreach (JsonElement tarkovItemCategoryJson in tarkovItemCategoriesJson.EnumerateArray())
            {
                tarkovItemCategories.Add(tarkovItemCategoryJson.GetProperty("id").GetString());
            }

            ItemCategory itemCategory = await ItemCategoryFinder.FindFromTarkovCategoryId(tarkovItemCategories.First());

            switch (itemCategory.Id)
            {
                case "ammunition":
                    return DeserializeAmmunition(itemJson, itemCategory.Id);
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
                    string error = Properties.Resources.NotImplementedItemCategory;
                    Logger.LogError(error);

                    throw new NotImplementedException(error);
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

            JsonElement propertiesJson = itemJson.GetProperty("properties");
            eyewear.BlindnessProtectionPercentage = propertiesJson.GetProperty("blindnessProtection").GetDouble();

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
            
            JsonElement propertiesJson = itemJson.GetProperty("properties");
            grenade.ExplosionDelay = propertiesJson.GetProperty("fuse").GetDouble();
            grenade.FragmentsAmount = propertiesJson.GetProperty("fragments").GetDouble();
            grenade.MaximumExplosionRange = propertiesJson.GetProperty("maxExplosionDistance").GetDouble();
            grenade.MinimumExplosionRange = propertiesJson.GetProperty("minExplosionDistance").GetDouble();
            grenade.Type = propertiesJson.GetProperty("type").GetString();

            if (grenade.MaximumExplosionRange == 0)
            {
                grenade.MaximumExplosionRange = propertiesJson.GetProperty("contusionRadius").GetDouble();
            }

            if (grenade.MinimumExplosionRange == 0)
            {
                grenade.MinimumExplosionRange = propertiesJson.GetProperty("contusionRadius").GetDouble();
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

            JsonElement propertiesJson = itemJson.GetProperty("properties");
            headwear.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
            headwear.ArmoredAreas = GetArmoredAreas(propertiesJson);
            headwear.Deafening = propertiesJson.GetProperty("deafening").GetString();
            headwear.Durability = propertiesJson.GetProperty("durability").GetDouble();
            headwear.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
            headwear.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString();
            headwear.ModSlots = ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == headwear.Id)?.ModSlots ?? Array.Empty<ModSlot>();
            headwear.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
            //headwear.RicochetChance = ; // TODO : MISSING FROM API
            headwear.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();

            return headwear;
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
        /// Deserilizes a <see cref="Magazine"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="Magazine"/>.</returns>
        private Item DeserializeMagazine(JsonElement itemJson, string itemCategoryId)
        {
            Magazine magazine = DeserializeBaseItemProperties<Magazine>(itemJson, itemCategoryId);

            JsonElement propertiesJson = itemJson.GetProperty("properties");
            magazine.AcceptedAmmunitionIds = ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == magazine.Id)?.AcceptedAmmunitionIds ?? Array.Empty<string>();; // TODO : MISSING FROM API
            magazine.Capacity = propertiesJson.GetProperty("capacity").GetDouble();
            magazine.CheckSpeedPercentageModifier = propertiesJson.GetProperty("ammoCheckModifier").GetDouble();
            magazine.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
            magazine.LoadSpeedPercentageModifier = propertiesJson.GetProperty("loadModifier").GetDouble();
            magazine.MalfunctionPercentage = propertiesJson.GetProperty("malfunctionChance").GetDouble();
            magazine.ModSlots = ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == magazine.Id)?.ModSlots ?? Array.Empty<ModSlot>();

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
            
            JsonElement propertiesJson = itemJson.GetProperty("properties");
            meleeWeapon.ChopDamage = propertiesJson.GetProperty("slashDamage").GetDouble();
            meleeWeapon.HitRadius = propertiesJson.GetProperty("hitRadius").GetDouble();
            meleeWeapon.StabDamage = propertiesJson.GetProperty("stabDamage").GetDouble();

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

            JsonElement propertiesJson = itemJson.GetProperty("properties");
            mod.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
            mod.ModSlots = ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == mod.Id)?.ModSlots ?? Array.Empty<ModSlot>();

            return mod;
        }

        /// <summary>
        /// Deserilizes a <see cref="RangedWeapon"/>.
        /// </summary>
        /// <param name="itemJson">Json element representing the item to deserialize.</param>
        /// <param name="itemCategoryId">ID of the item category ID the item belongs to.</param>
        /// <returns>Deserialized <see cref="RangedWeapon"/>.</returns>
        private Item DeserializeRangedWeapon(JsonElement itemJson, string itemCategoryId)
        {
            RangedWeapon rangedWeapon = DeserializeBaseItemProperties<RangedWeapon>(itemJson, itemCategoryId);

            JsonElement propertiesJson = itemJson.GetProperty("properties");
            rangedWeapon.Caliber = propertiesJson.GetProperty("caliber").GetString();
            rangedWeapon.Ergonomics = propertiesJson.GetProperty("ergonomics").GetDouble();
            rangedWeapon.FireModes = propertiesJson.GetProperty("fireModes").EnumerateArray().Select((JsonElement fireModeJson) => fireModeJson.GetString().ToStringCase()).ToArray();
            rangedWeapon.FireRate = propertiesJson.GetProperty("fireRate").GetDouble();
            rangedWeapon.HorizontalRecoil = propertiesJson.GetProperty("recoilHorizontal").GetDouble();
            rangedWeapon.ModSlots = ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == rangedWeapon.Id)?.ModSlots ?? Array.Empty<ModSlot>();
            rangedWeapon.VerticalRecoil = propertiesJson.GetProperty("recoilVertical").GetDouble();

            return rangedWeapon;
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

            JsonElement propertiesJson = itemJson.GetProperty("properties");

            if (propertiesJson.TryGetProperty("accuracyModifier", out JsonElement accuracyModifierJson))
            {
                rangedWeaponMod.AccuracyPercentageModifier = accuracyModifierJson.GetDouble();
            }

            rangedWeaponMod.ErgonomicsModifier = propertiesJson.GetProperty("ergonomics").GetDouble();
            rangedWeaponMod.ModSlots = ItemMissingProperties.FirstOrDefault(ifmp => ifmp.Id == rangedWeaponMod.Id)?.ModSlots ?? Array.Empty<ModSlot>();
            rangedWeaponMod.RecoilPercentageModifier = propertiesJson.GetProperty("recoilModifier").GetDouble();

            return rangedWeaponMod;
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

            JsonElement propertiesJson = itemJson.GetProperty("properties");
            vest.ArmorClass = propertiesJson.GetProperty("class").GetDouble();
            vest.ArmoredAreas = GetArmoredAreas(propertiesJson);
            vest.Capacity = propertiesJson.GetProperty("capacity").GetDouble();
            vest.Durability = propertiesJson.GetProperty("durability").GetDouble();
            vest.ErgonomicsPercentageModifier = propertiesJson.GetProperty("ergoPenalty").GetDouble() / 100;
            vest.Material = propertiesJson.GetProperty("material").GetProperty("name").GetString();
            vest.MovementSpeedPercentageModifier = propertiesJson.GetProperty("speedPenalty").GetDouble();
            //vest.RicochetChance = ; // TODO : MISSING FROM API
            vest.TurningSpeedPercentageModifier = propertiesJson.GetProperty("turnPenalty").GetDouble();

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
