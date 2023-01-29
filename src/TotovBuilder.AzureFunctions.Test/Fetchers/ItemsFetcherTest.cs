using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="ItemsFetcher"/> class.
    /// </summary>
    public class ItemsFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnItems()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiItemsQuery = "{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.ItemsJson) }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>?>(TestData.ItemMissingProperties));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>?>(TestData.ArmorPenetrations));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<TarkovValues?>(TestData.TarkovValues));

            ItemsFetcher fetcher = new(
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object,
                armorPenetrationsFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            IEnumerable<Item>? result = (await fetcher.Fetch())?.OrderBy(i => i.Id);

            // Assert
            IEnumerable<Item> expected = TestData.Items.OrderBy(i => i.Id);
            result.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithoutItemMissingProperties_ShouldReturnItems()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiItemsQuery = "{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.ItemsJson) }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<Item>>(It.IsAny<DataType>())).Returns(value: null);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>?>(null));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>?>(null));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<TarkovValues?>(null));

            ItemsFetcher fetcher = new(
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object,
                armorPenetrationsFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            IEnumerable<Item>? result = (await fetcher.Fetch())?.OrderBy(i => i.Id);

            // Assert
            Item[] expected = new Item[]
            {
                new Ammunition()
                {
                    AccuracyPercentageModifier = -0.05,
                    ArmorDamagePercentage = 0.76,
                    ArmorPenetrations = Array.Empty<double>(),
                    Caliber = "Caliber762x39",
                    CategoryId = "ammunition",
                    DurabilityBurnPercentageModifier = 0.7,
                    FleshDamage = 47,
                    FragmentationChancePercentage = 0.05,
                    HeavyBleedingPercentageChance = 0.1,
                    IconLink = "https://assets.tarkov.dev/601aa3d2b2bcb34913271e6d-icon.jpg",
                    Id = "601aa3d2b2bcb34913271e6d",
                    ImageLink = "https://assets.tarkov.dev/601aa3d2b2bcb34913271e6d-image.jpg",
                    LightBleedingPercentageChance = 0.1,
                    MarketLink = "https://tarkov.dev/item/762x39mm-mai-ap",
                    MaxStackableAmount = 60,
                    Name = "7.62x39mm MAI AP",
                    PenetrationPower = 58,
                    Projectiles = 1,
                    RecoilPercentageModifier = 0.10,
                    ShortName = "MAI AP",
                    Subsonic = false,
                    Tracer = false,
                    Velocity = 730,
                    Weight = 0.012,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/7.62x39mm_MAI_AP"
                },
                new Ammunition()
                {
                    AccuracyPercentageModifier = -0.15,
                    ArmorDamagePercentage = 0.26,
                    ArmorPenetrations = Array.Empty<double>(),
                    Caliber = "Caliber12g",
                    CategoryId = "ammunition",
                    DurabilityBurnPercentageModifier = 0,
                    FleshDamage = 50,
                    FragmentationChancePercentage = 0,
                    HeavyBleedingPercentageChance = 0.1,
                    IconLink = "https://assets.tarkov.dev/5d6e6806a4b936088465b17e-icon.jpg",
                    Id = "5d6e6806a4b936088465b17e",
                    ImageLink = "https://assets.tarkov.dev/5d6e6806a4b936088465b17e-image.jpg",
                    LightBleedingPercentageChance = 0.2,
                    MarketLink = "https://tarkov.dev/item/1270-85mm-magnum-buckshot",
                    MaxStackableAmount = 20,
                    Name = "12/70 8.5mm Magnum buckshot",
                    PenetrationPower = 2,
                    Projectiles = 8,
                    RecoilPercentageModifier = 0.15,
                    ShortName = "Magnum",
                    Subsonic = false,
                    Tracer = false,
                    Velocity = 385,
                    Weight = 0.059,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/12/70_8.5mm_Magnum_buckshot"
                },
                new Ammunition()
                {
                    AccuracyPercentageModifier = 0,
                    ArmorDamagePercentage = 0.34,
                    ArmorPenetrations = Array.Empty<double>(),
                    Caliber = "Caliber545x39",
                    CategoryId = "ammunition",
                    DurabilityBurnPercentageModifier = -0.2,
                    FleshDamage = 65,
                    FragmentationChancePercentage = 0.1,
                    HeavyBleedingPercentageChance = 0,
                    IconLink = "https://assets.tarkov.dev/56dff4ecd2720b5f5a8b4568-icon.jpg",
                    Id = "56dff4ecd2720b5f5a8b4568",
                    ImageLink = "https://assets.tarkov.dev/56dff4ecd2720b5f5a8b4568-image.jpg",
                    LightBleedingPercentageChance = 0,
                    MarketLink = "https://tarkov.dev/item/545x39mm-us-gs",
                    MaxStackableAmount = 60,
                    Name = "5.45x39mm US gs",
                    PenetrationPower = 15,
                    Projectiles = 1,
                    RecoilPercentageModifier = -0.25,
                    ShortName = "US",
                    Subsonic = true,
                    Tracer = false,
                    Velocity = 303,
                    Weight = 0.01,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/5.45x39mm_US_gs"
                },
                new Ammunition()
                {
                    CategoryId = "ammunition",
                    Id = "testAmmunition",
                },
                new Armor()
                {
                    ArmorClass = 6,
                    ArmoredAreas = new string[]
                    {
                        "LeftArm",
                        "RightArm",
                        "Thorax",
                        "Stomach"
                    },
                    CategoryId = "armor",
                    Durability = 85,
                    ErgonomicsPercentageModifier = -0.27,
                    IconLink = "https://assets.tarkov.dev/545cdb794bdc2d3a198b456a-icon.jpg",
                    Id = "545cdb794bdc2d3a198b456a",
                    ImageLink = "https://assets.tarkov.dev/545cdb794bdc2d3a198b456a-image.jpg",
                    MarketLink = "https://tarkov.dev/item/6b43-6a-zabralo-sh-body-armor",
                    Material = "CombinedMaterials",
                    MovementSpeedPercentageModifier = -0.35,
                    Name = "6B43 6A Zabralo-Sh body armor",
                    ShortName = "6B43 6A",
                    TurningSpeedPercentageModifier = -0.21,
                    Weight = 20,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/6B43_6A_Zabralo-Sh_body_armor"
                },
                new Armor()
                {
                    CategoryId = "armor",
                    Id = "testArmor",
                },
                new ArmorMod()
                {
                    ArmorClass = 3,
                    ArmoredAreas = new string[]
                    {
                        "Eyes",
                        "Jaws"
                    },
                    BlindnessProtectionPercentage = 0.1,
                    CategoryId = "armorMod",
                    ConflictingItemIds = Array.Empty<string>(),
                    Durability = 40,
                    ErgonomicsPercentageModifier = -0.08,
                    IconLink = "https://assets.tarkov.dev/5a16b7e1fcdbcb00165aa6c9-icon.jpg",
                    Id = "5a16b7e1fcdbcb00165aa6c9",
                    ImageLink = "https://assets.tarkov.dev/5a16b7e1fcdbcb00165aa6c9-image.jpg",
                    MarketLink = "https://tarkov.dev/item/ops-core-fast-multi-hit-ballistic-face-shield",
                    Material = "Glass",
                    MovementSpeedPercentageModifier = 0,
                    Name = "Ops-Core FAST multi-hit ballistic face shield",
                    ShortName = "FAST FS",
                    TurningSpeedPercentageModifier = -0.08,
                    Weight = 1.2,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Ops-Core_FAST_multi-hit_ballistic_face_shield",
                },
                new ArmorMod()
                {
                    CategoryId = "armorMod",
                    Id = "testArmorMod",
                },
                new Container()
                {
                    Capacity = 35,
                    CategoryId = "backpack",
                    IconLink = "https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-icon.jpg",
                    Id = "5ab8ebf186f7742d8b372e80",
                    ImageLink = "https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-image.jpg",
                    MarketLink = "https://tarkov.dev/item/sso-attack-2-raid-backpack",
                    Name = "SSO Attack 2 raid backpack",
                    ShortName = "Attack 2",
                    Weight = 6.12,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/SSO_Attack_2_raid_backpack"
                },
                new Container()
                {
                    Capacity = 4,
                    CategoryId = "container",
                    IconLink = "https://assets.tarkov.dev/5783c43d2459774bbe137486-icon.jpg",
                    Id = "5783c43d2459774bbe137486",
                    ImageLink = "https://assets.tarkov.dev/5783c43d2459774bbe137486-image.jpg",
                    MarketLink = "https://tarkov.dev/item/simple-wallet",
                    Name = "Simple wallet",
                    ShortName = "Wallet",
                    Weight = 0.23,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Simple_wallet"
                },
                new Container()
                {
                    Capacity = 12,
                    CategoryId = "securedContainer",
                    IconLink = "https://assets.tarkov.dev/5c093ca986f7740a1867ab12-icon.jpg",
                    Id = "5c093ca986f7740a1867ab12",
                    ImageLink = "https://assets.tarkov.dev/5c093ca986f7740a1867ab12-image.jpg",
                    MarketLink = "https://tarkov.dev/item/secure-container-kappa",
                    Name = "Secure container Kappa",
                    ShortName = "Kappa",
                    Weight = 2,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Secure_container_Kappa"
                },
                new Container()
                {
                    CategoryId = "securedContainer",
                    Id = "testContainer",
                },
                new Eyewear()
                {
                    BlindnessProtectionPercentage = 0.1,
                    CategoryId = "eyewear",
                    IconLink = "https://assets.tarkov.dev/5b432be65acfc433000ed01f-icon.jpg",
                    Id = "5b432be65acfc433000ed01f",
                    ImageLink = "https://assets.tarkov.dev/5b432be65acfc433000ed01f-image.jpg",
                    MarketLink = "https://tarkov.dev/item/6b34-anti-fragmentation-glasses",
                    Name = "6B34 anti-fragmentation glasses",
                    ShortName = "6B34",
                    Weight = 0.12,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/6B34_anti-fragmentation_glasses"
                },
                new Eyewear()
                {
                    CategoryId = "eyewear",
                    Id = "testEyewear",
                },
                new Grenade()
                {
                    CategoryId = "grenade",
                    ExplosionDelay = 3,
                    FragmentsAmount = 100,
                    MaximumExplosionRange = 6,
                    MinimumExplosionRange = 2,
                    IconLink = "https://assets.tarkov.dev/5e32f56fcb6d5863cc5e5ee4-icon.jpg",
                    Id = "5e32f56fcb6d5863cc5e5ee4",
                    ImageLink = "https://assets.tarkov.dev/5e32f56fcb6d5863cc5e5ee4-image.jpg",
                    MarketLink = "https://tarkov.dev/item/vog-17-khattabka-improvised-hand-grenade",
                    Name = "VOG-17 Khattabka improvised hand grenade",
                    ShortName = "VOG-17",
                    Type = "Grenade",
                    Weight = 0.28,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/VOG-17_Khattabka_improvised_hand_grenade"
                },
                new Grenade()
                {
                    CategoryId = "grenade",
                    ExplosionDelay = 2,
                    FragmentsAmount = 0,
                    MaximumExplosionRange = 10,
                    MinimumExplosionRange = 10,
                    IconLink = "https://assets.tarkov.dev/5a0c27731526d80618476ac4-icon.jpg",
                    Id = "5a0c27731526d80618476ac4",
                    ImageLink = "https://assets.tarkov.dev/5a0c27731526d80618476ac4-image.jpg",
                    MarketLink = "https://tarkov.dev/item/zarya-stun-grenade",
                    Name = "\"Zarya\" stun grenade",
                    ShortName = "Zarya",
                    Type = "Flashbang",
                    Weight = 0.175,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/%22Zarya%22_stun_grenade"
                },
                new Grenade()
                {
                    CategoryId = "grenade",
                    Id = "testGrenade",
                },
                new Headwear()
                {
                    ArmorClass = 4,
                    ArmoredAreas = new string[]
                    {
                        "Top",
                        "Nape"
                    },
                    CategoryId = "headwear",
                    Deafening = "None",
                    Durability = 30,
                    ErgonomicsPercentageModifier = -0.06,
                    IconLink = "https://assets.tarkov.dev/5e4bfc1586f774264f7582d3-icon.jpg",
                    Id = "5e4bfc1586f774264f7582d3",
                    ImageLink = "https://assets.tarkov.dev/5e4bfc1586f774264f7582d3-image.jpg",
                    MarketLink = "https://tarkov.dev/item/msa-gallet-tc-800-high-cut-combat-helmet",
                    Material = "CombinedMaterials",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5a16b672fcdbcb001912fa83",
                                "5a16b7e1fcdbcb00165aa6c9"
                            },
                            Name = "mod_equipment_000",
                            Required = false
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5c0558060db834001b735271",
                                "5a16b8a9fcdbcb00165aa6ca"
                            },
                            Name = "mod_nvg",
                            Required = false
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5a398b75c4a282000a51a266",
                                "5a398ab9c4a282000c5a9842"
                            },
                            Name = "mod_mount",
                            Required = false
                        }
                    },
                    MovementSpeedPercentageModifier = -0.02,
                    Name = "MSA Gallet TC 800 High Cut combat helmet",
                    RicochetChance = string.Empty,
                    ShortName = "TC 800",
                    TurningSpeedPercentageModifier = -0.08,
                    Weight = 1.17,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MSA_Gallet_TC_800_High_Cut_combat_helmet"
                },
                new Headwear()
                {
                    ArmorClass = 0,
                    ArmoredAreas = Array.Empty<string>(),
                    CategoryId = "headwear",
                    Deafening = "None",
                    Durability = 0,
                    ErgonomicsPercentageModifier = 0,
                    IconLink = "https://assets.tarkov.dev/5bd073c986f7747f627e796c-icon.jpg",
                    Id = "5bd073c986f7747f627e796c",
                    ImageLink = "https://assets.tarkov.dev/5bd073c986f7747f627e796c-image.jpg",
                    MarketLink = "https://tarkov.dev/item/kotton-beanie",
                    Material = string.Empty,
                    ModSlots = Array.Empty<ModSlot>(),
                    MovementSpeedPercentageModifier = 0,
                    Name = "Kotton beanie",
                    RicochetChance = string.Empty,
                    ShortName = "Kotton",
                    TurningSpeedPercentageModifier = 0,
                    Weight = 0.2,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Kotton_beanie"
                },
                new Headwear()
                {
                    CategoryId = "headwear",
                    Id = "testHeadwear",
                },
                new Headwear()
                {
                    CategoryId = "headwear",
                    Deafening = "None",
                    IconLink = "https://assets.tarkov.dev/5a16bb52fcdbcb001a3b00dc-icon.jpg",
                    Id = "5a16bb52fcdbcb001a3b00dc",
                    ImageLink = "https://assets.tarkov.dev/5a16bb52fcdbcb001a3b00dc-image.jpg",
                    MarketLink = "https://tarkov.dev/item/wilcox-skull-lock-head-mount",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5a16b8a9fcdbcb00165aa6ca",
                            },
                            Name = "mod_nvg",
                            Required = false // TODO : MISSING FROM API
                        }
                    },
                    Name = "Wilcox Skull Lock head mount",
                    ShortName = "Skull Lock",
                    Weight = 0.5,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Wilcox_Skull_Lock_head_mount"
                },
                new Item()
                {
                    CategoryId = "armband",
                    IconLink = "https://assets.tarkov.dev/5f9949d869e2777a0e779ba5-icon.jpg",
                    Id = "5f9949d869e2777a0e779ba5",
                    ImageLink = "https://assets.tarkov.dev/5f9949d869e2777a0e779ba5-image.jpg",
                    MarketLink = "https://tarkov.dev/item/rivals-2020-armband",
                    Name = "Rivals 2020 armband",
                    ShortName = "Rivals",
                    Weight = 0.05,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Rivals_2020_armband"
                },
                new Item()
                {
                    CategoryId = "currency",
                    IconLink = "https://assets.tarkov.dev/569668774bdc2da2298b4568-icon.jpg",
                    Id = "569668774bdc2da2298b4568",
                    ImageLink = "https://assets.tarkov.dev/569668774bdc2da2298b4568-image.jpg",
                    MarketLink = "https://tarkov.dev/item/euros",
                    Name = "Euros",
                    ShortName = "EUR",
                    Weight = 0,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Euros"
                },
                new Item()
                {
                    CategoryId = "faceCover",
                    IconLink = "https://assets.tarkov.dev/5e54f76986f7740366043752-icon.jpg",
                    Id = "5e54f76986f7740366043752",
                    ImageLink = "https://assets.tarkov.dev/5e54f76986f7740366043752-image.jpg",
                    MarketLink = "https://tarkov.dev/item/shroud-half-mask",
                    Name = "Shroud half-mask",
                    ShortName = "Shroud",
                    Weight = 0.1,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Shroud_half-mask"
                },
                new Item()
                {
                    CategoryId = "headphones",
                    IconLink = "https://assets.tarkov.dev/628e4e576d783146b124c64d-icon.jpg",
                    Id = "628e4e576d783146b124c64d",
                    ImageLink = "https://assets.tarkov.dev/628e4e576d783146b124c64d-image.jpg",
                    MarketLink = "https://tarkov.dev/item/peltor-comtac-4-hybrid-headset",
                    Name = "Peltor ComTac 4 Hybrid headset",
                    ShortName = "ComTac 4",
                    Weight = 0.6,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Peltor_ComTac_4_Hybrid_headset"
                },
                new Item()
                {
                    CategoryId = "other",
                    IconLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg",
                    Id = "5c1d0c5f86f7744bb2683cf0",
                    ImageLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg",
                    MarketLink = "https://tarkov.dev/item/terragroup-labs-keycard-blue",
                    Name = "TerraGroup Labs keycard (Blue)",
                    ShortName = "Blue",
                    Weight = 0.01,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)"
                },
                new Item()
                {
                    CategoryId = "special",
                    IconLink = "https://assets.tarkov.dev/5991b51486f77447b112d44f-icon.jpg",
                    Id = "5991b51486f77447b112d44f",
                    ImageLink = "https://assets.tarkov.dev/5991b51486f77447b112d44f-image.jpg",
                    MarketLink = "https://tarkov.dev/item/ms2000-marker",
                    Name = "MS2000 Marker",
                    ShortName = "MS2000",
                    Weight = 0.5,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MS2000_Marker"
                },
                new Magazine()
                {
                    AcceptedAmmunitionIds = new string[]
                    {
                        "5e81f423763d9f754677bf2e",
                        "5efb0cabfb3e451d70735af5",
                        "5efb0fc6aeb21837e749c801",
                        "5efb0d4f4bc50b58e81710f3",
                        "5ea2a8e200685063ec28c05a"
                    },
                    Capacity = 7,
                    CategoryId = "magazine",
                    CheckSpeedPercentageModifier = -0.2,
                    ConflictingItemIds = Array.Empty<string>(), // TODO : MISSING FROM API
                    ErgonomicsModifier = -1,
                    IconLink = "https://assets.tarkov.dev/5e81c4ca763d9f754677befa-icon.jpg",
                    Id = "5e81c4ca763d9f754677befa",
                    ImageLink = "https://assets.tarkov.dev/5e81c4ca763d9f754677befa-image.jpg",
                    LoadSpeedPercentageModifier = -0.25,
                    MalfunctionPercentage = 0.04,
                    MarketLink = "https://tarkov.dev/item/m1911a1-45-acp-7-round-magazine",
                    Name = "M1911A1 .45 ACP 7-round magazine",
                    ShortName = "1911",
                    Weight = 0.16,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/M1911A1_.45_ACP_7-round_magazine"
                },
                new Magazine()
                {
                    CategoryId = "magazine",
                    Id = "testMagazine",
                },
                new Magazine()
                {
                    AcceptedAmmunitionIds = new string[]
                    {
                        "5c0d5e4486f77478390952fe",
                        "61962b617c6c7b169525f168",
                        "56dfef82d2720bbd668b4567",
                        "56dff026d2720bb8668b4567",
                        "56dff061d2720bb5668b4567",
                        "56dff0bed2720bb0668b4567",
                        "56dff216d2720bbd668b4568",
                        "56dff2ced2720bb4668b4567",
                        "56dff338d2720bbd668b4569",
                        "56dff3afd2720bba668b4567",
                        "56dff421d2720b5f5a8b4567",
                        "56dff4a2d2720bbd668b456a",
                        "56dff4ecd2720b5f5a8b4568"
                    },
                    Capacity = 30,
                    CategoryId = "magazine",
                    ErgonomicsModifier = -3,
                    IconLink = "https://assets.tarkov.dev/564ca99c4bdc2d16268b4589-icon.jpg",
                    Id = "564ca99c4bdc2d16268b4589",
                    ImageLink = "https://assets.tarkov.dev/564ca99c4bdc2d16268b4589-image.jpg",
                    MalfunctionPercentage = 0.07,
                    MarketLink = "https://tarkov.dev/item/ak-74-545x39-6l20-30-round-magazine",
                    Name = "AK-74 5.45x39 6L20 30-round magazine",
                    ShortName = "6L20",
                    Weight = 0.215,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AK-74_5.45x39_6L20_30-round_magazine"
                },
                new Magazine()
                {
                    AcceptedAmmunitionIds = new string[]
                    {
                        "5efb0da7a29a85116f6ea05f",
                        "5c3df7d588a4501f290594e5",
                        "58864a4f2459770fcc257101",
                        "56d59d3ad2720bdb418b4577",
                        "5c925fa22e221601da359b7b",
                        "5a3c16fe86f77452b62de32a",
                        "5efb0e16aeb21837e749c7ff",
                        "5c0d56a986f774449d5de529"
                    },
                    Capacity = 30,
                    CategoryId = "magazine",
                    CheckSpeedPercentageModifier = -0.5,
                    ErgonomicsModifier = -3,
                    IconLink = "https://assets.tarkov.dev/5894a05586f774094708ef75-icon.jpg",
                    Id = "5894a05586f774094708ef75",
                    ImageLink = "https://assets.tarkov.dev/5894a05586f774094708ef75-image.jpg",
                    MalfunctionPercentage = 0.05,
                    MarketLink = "https://tarkov.dev/item/mpx-9x19-30-round-magazine",
                    Name = "MPX 9x19 30-round magazine",
                    ShortName = "MPX",
                    Weight = 0.17,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MPX_9x19_30-round_magazine"
                },
                new MeleeWeapon()
                {
                    CategoryId = "meleeWeapon",
                    ChopDamage = 25,
                    HitRadius = 0.6,
                    IconLink = "https://assets.tarkov.dev/5c0126f40db834002a125382-icon.jpg",
                    Id = "5c0126f40db834002a125382",
                    ImageLink = "https://assets.tarkov.dev/5c0126f40db834002a125382-image.jpg",
                    MarketLink = "https://tarkov.dev/item/red-rebel-ice-pick",
                    Name = "Red Rebel ice pick",
                    ShortName = "RedRebel",
                    StabDamage = 30,
                    Weight = 0.65,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Red_Rebel_ice_pick"
                },
                new MeleeWeapon()
                {
                    CategoryId = "meleeWeapon",
                    Id = "testMeleeWeapon",
                },
                new Mod()
                {
                    CategoryId = "mod",
                    ErgonomicsModifier = -2,
                    IconLink = "https://assets.tarkov.dev/59d790f486f77403cb06aec6-icon.jpg",
                    Id = "59d790f486f77403cb06aec6",
                    ImageLink = "https://assets.tarkov.dev/59d790f486f77403cb06aec6-image.jpg",
                    MarketLink = "https://tarkov.dev/item/armytek-predator-pro-v3-xhp35-hi-flashlight",
                    Name = "Armytek Predator Pro v3 XHP35 HI flashlight",
                    ShortName = "XHP35",
                    Weight = 0.12,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Armytek_Predator_Pro_v3_XHP35_HI_flashlight"
                },
                new Mod()
                {
                    CategoryId = "mod",
                    ErgonomicsModifier = -1,
                    IconLink = "https://assets.tarkov.dev/57d17e212459775a1179a0f5-icon.jpg",
                    Id = "57d17e212459775a1179a0f5",
                    ImageLink = "https://assets.tarkov.dev/57d17e212459775a1179a0f5-image.jpg",
                    MarketLink = "https://tarkov.dev/item/kiba-arms-25mm-accessory-ring-mount",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "59d790f486f77403cb06aec6",
                                "57d17c5e2459775a5c57d17d"
                            },
                            Name = "mod_flashlight"
                        }
                    },
                    Name = "Kiba Arms 25mm accessory ring mount",
                    ShortName = "25mm ring",
                    Weight = 0.085,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Kiba_Arms_25mm_accessory_ring_mount"
                },
                new Mod()
                {
                    CategoryId = "mod",
                    Id = "testMod",
                },
                new Mod()
                {
                    CategoryId = "mod",
                    IconLink = "https://assets.tarkov.dev/58a56f8d86f774651579314c-icon.jpg",
                    Id = "58a56f8d86f774651579314c",
                    ImageLink = "https://assets.tarkov.dev/58a56f8d86f774651579314c-image.jpg",
                    MarketLink = "https://tarkov.dev/item/mpx-gen1-handguard-2-inch-rail",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5a800961159bd4315e3a1657",
                                "57fd23e32459772d0805bcf1",
                                "544909bb4bdc2d6f028b4577",
                                "5c06595c0db834001a66af6c",
                                "5cc9c20cd7f00c001336c65d",
                                "5d2369418abbc306c62e0c80",
                                "5b07dd285acfc4001754240d",
                                "56def37dd2720bec348b456a",
                                "5a7b483fe899ef0016170d15",
                                "61605d88ffa6e502ac5e7eeb",
                                "5a5f1ce64f39f90b401987bc",
                                "560d657b4bdc2da74d8b4572",
                                "5b3a337e5acfc4704b4a19a0",
                                "5c5952732e2216398b5abda2",
                                "57d17e212459775a1179a0f5",
                                "6267c6396b642f77f56f5c1c",
                                "6272370ee4013c5d7e31f418",
                                "6272379924e29f06af4d5ecb",
                                "626becf9582c3e319310b837"
                            },
                            Name = "mod_tactical",
                        }
                    },
                    Name = "MPX GEN1 handguard 2 inch rail",
                    ShortName = "MPX 2\"",
                    Weight = 0.07,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MPX_GEN1_handguard_2_inch_rail"
                },
                new Mod()
                {
                    CategoryId = "mod",
                    IconLink = "https://assets.tarkov.dev/58a5c12e86f7745d585a2b9e-icon.jpg",
                    Id = "58a5c12e86f7745d585a2b9e",
                    ImageLink = "https://assets.tarkov.dev/58a5c12e86f7745d585a2b9e-image.jpg",
                    MarketLink = "https://tarkov.dev/item/mpx-gen1-handguard-4-inch-rail",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5c7fc87d2e221644f31c0298",
                                "5cda9bcfd7f00c0c0b53e900",
                                "59f8a37386f7747af3328f06",
                                "619386379fb0c665d5490dbe",
                                "5c87ca002e221600114cb150",
                                "588226d124597767ad33f787",
                                "588226dd24597767ad33f789",
                                "588226e62459776e3e094af7",
                                "588226ef24597767af46e39c",
                                "59fc48e086f77463b1118392",
                                "5fce0cf655375d18a253eff0",
                                "5cf4fb76d7f00c065703d3ac",
                                "5b057b4f5acfc4771e1bd3e9",
                                "5c791e872e2216001219c40a",
                                "558032614bdc2de7118b4585",
                                "58c157be86f77403c74b2bb6",
                                "58c157c886f774032749fb06",
                                "5f6340d3ca442212f4047eb2",
                                "591af28e86f77414a27a9e1d",
                                "5c1cd46f2e22164bef5cfedb",
                                "5c1bc4812e22164bef5cfde7",
                                "5c1bc5612e221602b5429350",
                                "5c1bc5af2e221602b412949b",
                                "5c1bc5fb2e221602b1779b32",
                                "5c1bc7432e221602b412949d",
                                "5c1bc7752e221602b1779b34"
                            },
                            Name = "mod_foregrip"
                        }
                    },
                    Name = "MPX GEN1 handguard 4 inch rail",
                    ShortName = "MPX 4\"",
                    Weight = 0.05,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MPX_GEN1_handguard_4_inch_rail"
                },
                new Mod()
                {
                    CategoryId = "mod",
                    IconLink = "https://assets.tarkov.dev/58d2664f86f7747fec5834f6-icon.jpg",
                    Id = "58d2664f86f7747fec5834f6",
                    ImageLink = "https://assets.tarkov.dev/58d2664f86f7747fec5834f6-image.jpg",
                    MarketLink = "https://tarkov.dev/item/deltapoint-cross-slot-mount-base",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "58d268fc86f774111273f8c2"
                            },
                            Name = "mod_scope"
                        }
                    },
                    Name = "DeltaPoint Cross Slot Mount base",
                    ShortName = "DPCSM",
                    Weight = 0.045,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/DeltaPoint_Cross_Slot_Mount_base"
                },
                new Mod()
                {
                    CategoryId = "mod",
                    IconLink = "https://assets.tarkov.dev/5a16b8a9fcdbcb00165aa6ca-icon.jpg",
                    Id = "5a16b8a9fcdbcb00165aa6ca",
                    ImageLink = "https://assets.tarkov.dev/5a16b8a9fcdbcb00165aa6ca-image.jpg",
                    MarketLink = "https://tarkov.dev/item/norotos-titanium-advanced-tactical-mount",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5c0695860db834001b735461",
                                "5a16b93dfcdbcbcae6687261",
                                "5c11046cd174af02a012e42b"
                            },
                            Name = "mod_nvg"
                        }
                    },
                    Name = "Norotos Titanium Advanced Tactical Mount",
                    ShortName = "TATM",
                    Weight = 0.07,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Norotos_Titanium_Advanced_Tactical_Mount"
                },
                new Mod()
                {
                    CategoryId = "mod",
                    IconLink = "https://assets.tarkov.dev/5a16b93dfcdbcbcae6687261-icon.jpg",
                    Id = "5a16b93dfcdbcbcae6687261",
                    ImageLink = "https://assets.tarkov.dev/5a16b93dfcdbcbcae6687261-image.jpg",
                    MarketLink = "https://tarkov.dev/item/anpvs-14-dual-dovetail-mount",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "57235b6f24597759bf5a30f1"
                            },
                            Name = "mod_nvg"
                        }
                    },
                    Name = "AN/PVS-14 Dual Dovetail Mount",
                    ShortName = "DDT",
                    Weight = 0.07,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AN/PVS-14_Dual_Dovetail_Mount"
                },
                new Mod()
                {
                    CategoryId = "mod",
                    IconLink = "https://assets.tarkov.dev/57235b6f24597759bf5a30f1-icon.jpg",
                    Id = "57235b6f24597759bf5a30f1",
                    ImageLink = "https://assets.tarkov.dev/57235b6f24597759bf5a30f1-image.jpg",
                    MarketLink = "https://tarkov.dev/item/anpvs-14-night-vision-monocular",
                    Name = "AN/PVS-14 Night Vision Monocular",
                    ShortName = "PVS-14",
                    Weight = 0.65,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AN/PVS-14_Night_Vision_Monocular"
                },
                new RangedWeapon()
                {
                    Caliber = "Caliber545x39",
                    CategoryId = "mainWeapon",
                    Ergonomics = 44,
                    FireModes = new string[] { "SingleFire", "FullAuto" },
                    FireRate = 650,
                    HorizontalRecoil = 445,
                    IconLink = "https://assets.tarkov.dev/57dc2fa62459775949412633-icon.jpg",
                    Id = "57dc2fa62459775949412633",
                    ImageLink = "https://assets.tarkov.dev/57dc2fa62459775949412633-image.jpg",
                    MarketLink = "https://tarkov.dev/item/kalashnikov-aks-74u-545x39-assault-rifle",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5f6341043ada5942720e2dc5",
                                "6087e663132d4d12c81fd96b",
                                "5beec8ea0db834001a6f9dbf",
                                "5649ad3f4bdc2df8348b4585",
                                "5649ade84bdc2d1b2b8b4587",
                                "59e62cc886f77440d40b52a1",
                                "5a0071d486f77404e23a12b2",
                                "57e3dba62459770f0c32322b",
                                "5cf54404d7f00c108840b2ef",
                                "5e2192a498a36665e8337386",
                                "5b30ac585acfc433000eb79c",
                                "59e6318286f77444dd62c4cc",
                                "5cf50850d7f00c056e24104c",
                                "5cf508bfd7f00c056e24104e",
                                "5947f92f86f77427344a76b1",
                                "5947fa2486f77425b47c1a9b",
                                "5c6bf4aa2e2216001219b0ae",
                                "5649ae4a4bdc2d1b2b8b4588",
                                "5998517986f7746017232f7e",
                                "623c3be0484b5003161840dc",
                                "628c9ab845c59e5b80768a81",
                                "628a664bccaab13006640e47"
                            },
                            Name = "mod_pistol_grip",
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "59ecc28286f7746d7a68aa8c",
                                "5ab626e4d8ce87272e4c6e43",
                                "57dc347d245977596754e7a1"
                            },
                            Name = "mod_stock",
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "6130ca3fd92c473c77020dbd",
                                "5648ac824bdc2ded0b8b457d"
                            },
                            Name = "mod_charge",
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "564ca9df4bdc2d35148b4569",
                                "564ca99c4bdc2d16268b4589",
                                "55d480c04bdc2d1d4e8b456a",
                                "5cbdaf89ae9215000e5b9c94",
                                "55d481904bdc2d8c2f8b456a",
                                "55d482194bdc2d1d4e8b456b",
                                "55d4837c4bdc2d1d4e8b456c",
                                "5aaa4194e5b5b055d06310a5",
                                "5bed61680db834001d2c45ab",
                                "5bed625c0db834001c062946"
                            },
                            Name = "mod_magazine"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5ac72e945acfc43f3b691116",
                                "5ac7655e5acfc40016339a19",
                                "5649aa744bdc2ded0b8b457e",
                                "5f633f791b231926f2329f13",
                                "5943eeeb86f77412d6384f6b",
                                "5cc9a96cd7f00c011c04e04a",
                                "615d8f5dd92c473c770212ef",
                                "5649ab884bdc2ded0b8b457f",
                                "57dc324a24597759501edc20",
                                "59bffc1f86f77435b128b872",
                                "593d493f86f7745e6b2ceb22",
                                "564caa3d4bdc2d17108b458e",
                                "57ffb0e42459777d047111c5"
                            },
                            Name = "mod_muzzle"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "57dc334d245977597164366f",
                                "5839a7742459773cf9693481"
                            },
                            Name = "mod_reciever"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "59d36a0086f7747e673f3946"
                            },
                            Name = "mod_gas_block"
                        }
                    },
                    Name = "Kalashnikov AKS-74U 5.45x39 assault rifle",
                    ShortName = "AKS-74U",
                    VerticalRecoil = 141,
                    Weight = 1.809,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Kalashnikov_AKS-74U_5.45x39_assault_rifle"
                },
                new RangedWeapon()
                {
                    Caliber = "Caliber1143x23ACP",
                    CategoryId = "secondaryWeapon",
                    Ergonomics = 75,
                    FireModes = new string[] { "SingleFire" },
                    FireRate = 30,
                    HorizontalRecoil = 355,
                    IconLink = "https://assets.tarkov.dev/5e81c3cbac2bb513793cdc75-icon.jpg",
                    Id = "5e81c3cbac2bb513793cdc75",
                    ImageLink = "https://assets.tarkov.dev/5e81c3cbac2bb513793cdc75-image.jpg",
                    MarketLink = "https://tarkov.dev/item/colt-m1911a1-45-acp-pistol",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5e81c519cb2b95385c177551",
                                "5f3e7801153b8571434a924c",
                                "5f3e77f59103d430b93f94c1"
                            },
                            Name = "mod_barrel",
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5e81c6bf763d9f754677beff",
                                "5ef366938cef260c0642acad",
                                "626a9cb151cb5849f6002890"
                            },
                            Name = "mod_pistol_grip"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5e81edc13397a21db957f6a1",
                                "5f3e7823ddc4f03b010e2045"
                            },
                            Name = "mod_reciever"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5e81c4ca763d9f754677befa",
                                "5f3e77b26cda304dcc634057",
                                "5ef3448ab37dfd6af863525c"
                            },
                            Name = "mod_magazine"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5ef32e4d1c1fd62aea6a150d",
                                "5e81c6a2ac2bb513793cdc7f",
                                "5f3e772a670e2a7b01739a52"
                            },
                            Name = "mod_trigger"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5e81c550763d9f754677befd",
                                "5f3e76d86cda304dcc634054",
                                "5ef35f46382a846010715a96",
                                "5ef35d2ac64c5d0dfc0571b0",
                                "5ef35bc243cb350a955a7ccd"
                            },
                            Name = "mod_hammer"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5e81c539cb2b95385c177553",
                                "5f3e777688ca2d00ad199d25",
                                "5ef3553c43cb350a955a7ccb"
                            },
                            Name = "mod_catch"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5ef5d994dfbc9f3c660ded95"
                            },
                            Name = "mod_mount_000"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5ef369b08cef260c0642acaf"
                            },
                            Name = "mod_mount_001"
                        }
                    },
                    Name = "Colt M1911A1 .45 ACP pistol",
                    ShortName = "M1911A1",
                    VerticalRecoil = 530,
                    Weight = 0.231,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Colt_M1911A1_.45_ACP_pistol"
                },
                new RangedWeapon()
                {
                    Caliber = "Caliber26x75",
                    CategoryId = "mainWeapon",
                    Ergonomics = 51,
                    FireModes = new string[] { "SingleFire" },
                    FireRate = 30,
                    HorizontalRecoil = 400,
                    IconLink = "https://assets.tarkov.dev/624c0b3340357b5f566e8766-icon.jpg",
                    Id = "624c0b3340357b5f566e8766",
                    ImageLink = "https://assets.tarkov.dev/624c0b3340357b5f566e8766-image.jpg",
                    MarketLink = "https://tarkov.dev/item/rsp-30-reactive-signal-cartridge-yellow",
                    Name = "RSP-30 reactive signal cartridge (Yellow)",
                    ShortName = "Yellow",
                    VerticalRecoil = 200,
                    Weight = 0.6,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/RSP-30_reactive_signal_cartridge_(Yellow)"
                },
                new RangedWeapon()
                {
                    CategoryId = "secondaryWeapon",
                    Id = "testRangedWeapon"
                },
                new RangedWeapon()
                {
                    Caliber = "Caliber9x19PARA",
                    CategoryId = "mainWeapon",
                    Ergonomics = 40,
                    FireModes = new string[] { "SingleFire", "FullAuto" },
                    FireRate = 850,
                    HorizontalRecoil = 299,
                    IconLink = "https://assets.tarkov.dev/58948c8e86f77409493f7266-icon.jpg",
                    Id = "58948c8e86f77409493f7266",
                    ImageLink = "https://assets.tarkov.dev/58948c8e86f77409493f7266-image.jpg",
                    MarketLink = "https://tarkov.dev/item/sig-mpx-9x19-submachine-gun",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "55d4b9964bdc2d1d4e8b456e",
                                "571659bb2459771fb2755a12",
                                "602e71bd53a60014f9705bfa",
                                "6113c3586c780c1e710c90bc",
                                "6113cc78d3a39d50044c065a",
                                "6113cce3d92c473c770200c7",
                                "5cc9bcaed7f00c011c04e179",
                                "5bb20e18d4351e00320205d5",
                                "5bb20e0ed4351e3bac1212dc",
                                "6193dcd0f8ee7e52e4210a28",
                                "5d025cc1d7ad1a53845279ef",
                                "5c6d7b3d2e221600114c9b7d",
                                "57c55efc2459772d2c6271e7",
                                "57af48872459771f0b2ebf11",
                                "57c55f092459772d291a8463",
                                "57c55f112459772d28133310",
                                "57c55f172459772d27602381",
                                "5a339805c4a2826c6e06d73d",
                                "55802f5d4bdc2dac148b458f",
                                "5d15cf3bd7ad1a67e71518b2",
                                "59db3a1d86f77429e05b4e92",
                                "5fbcbd6c187fea44d52eda14",
                                "59db3acc86f7742a2c4ab912",
                                "59db3b0886f77429d72fb895",
                                "615d8faecabb9b7ad90f4d5d",
                                "5b07db875acfc40dc528a5f6",
                                "5894a51286f77426d13baf02"
                            },
                            Name = "mod_pistol_grip"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5c5db6742e2216000f1b2852",
                                "5c5db6552e2216001026119d",
                                "5894a05586f774094708ef75",
                                "5c5db6652e221600113fba51"
                            },
                            Name = "mod_magazine"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5894a5b586f77426d2590767"
                            },
                            Name = "mod_reciever"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "58ac1bf086f77420ed183f9f",
                                "5894a13e86f7742405482982",
                                "5fbcc429900b1d5091531dd7",
                                "5fbcc437d724d907e2077d5c",
                                "5c5db6ee2e221600113fba54",
                                "5c5db6f82e2216003a0fe914"
                            },
                            Name = "mod_stock"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5c5db6b32e221600102611a0",
                                "58949edd86f77409483e16a9",
                                "58949fac86f77409483e16aa"
                            },
                            Name = "mod_charge"
                        }
                    },
                    Name = "SIG MPX 9x19 submachine gun",
                    ShortName = "MPX",
                    VerticalRecoil = 60,
                    Weight = 0.64,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/SIG_MPX_9x19_submachine_gun"
                },
                new RangedWeaponMod()
                {
                    AccuracyPercentageModifier = 0,
                    CategoryId = "rangedWeaponMod",
                    ConflictingItemIds = Array.Empty<string>(),
                    ErgonomicsModifier = 5,
                    IconLink = "https://assets.tarkov.dev/5d2c76ed48f03532f2136169-icon.jpg",
                    Id = "5d2c76ed48f03532f2136169",
                    ImageLink = "https://assets.tarkov.dev/5d2c76ed48f03532f2136169-image.jpg",
                    MarketLink = "https://tarkov.dev/item/ak-akademia-bastion-dust-cover",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "57ac965c24597706be5f975c",
                                "57aca93d2459771f2c7e26db",
                                "544a3a774bdc2d3a388b4567",
                                "5d2dc3e548f035404a1a4798",
                                "57adff4f24597737f373b6e6",
                                "5c0517910db83400232ffee5",
                                "591c4efa86f7741030027726",
                                "570fd79bd2720bc7458b4583",
                                "570fd6c2d2720bc6458b457f",
                                "558022b54bdc2dac148b458d",
                                "5c07dd120db834001c39092d",
                                "5c0a2cec0db834001b7ce47d",
                                "58491f3324597764bc48fa02",
                                "584924ec24597768f12ae244",
                                "5b30b0dc5acfc400153b7124",
                                "6165ac8c290d254f5e6b2f6c",
                                "60a23797a37c940de7062d02",
                                "5d2da1e948f035477b1ce2ba",
                                "5c0505e00db834001b735073",
                                "609a63b6e2ff132951242d09",
                                "584984812459776a704a82a6",
                                "59f9d81586f7744c7506ee62",
                                "570fd721d2720bc5458b4596",
                                "57ae0171245977343c27bfcf",
                                "5dfe6104585a0c3e995c7b82",
                                "5d1b5e94d7ad1a2b865a96b0",
                                "609bab8b455afd752b2e6138",
                                "58d39d3d86f77445bb794ae7",
                                "616554fe50224f204c1da2aa",
                                "5c7d55f52e221644f31bff6a",
                                "616584766ef05c2ce828ef57",
                                "5b3b6dc75acfc47a8773fb1e",
                                "615d8d878004cc50514c3233",
                                "5b2389515acfc4771e1be0c0",
                                "577d128124597739d65d0e56",
                                "618b9643526131765025ab35",
                                "618bab21526131765025ab3f",
                                "5c86592b2e2216000e69e77c",
                                "5a37ca54c4a282000d72296a",
                                "5d0a29fed7ad1a002769ad08",
                                "5c064c400db834001d23f468",
                                "58d2664f86f7747fec5834f6",
                                "57c69dd424597774c03b7bbc",
                                "5b3b99265acfc4704b4a1afb",
                                "5aa66a9be5b5b0214e506e89",
                                "5aa66c72e5b5b00016327c93",
                                "5c1cdd302e221602b3137250",
                                "61714b2467085e45ef140b2c",
                                "6171407e50224f204c1da3c5",
                                "61713cc4d8e3106d9806c109",
                                "5b31163c5acfc400153b71cb",
                                "5a33b652c4a28232996e407c",
                                "5a33b2c9c4a282000c5a9511",
                                "59db7eed86f77461f8380365",
                                "5a1ead28fcdbcb001912fa9f",
                                "5dff77c759400025ea5150cf",
                                "626bb8532c923541184624b4",
                                "62811f461d5df4475f46a332"
                            },
                            Name = "mod_scope"
                        }
                    },
                    Name = "AK AKademia Bastion dust cover",
                    RecoilPercentageModifier = -0.01,
                    ShortName = "Bastion",
                    Weight = 0.237,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AK_AKademia_Bastion_dust_cover"
                },
                new RangedWeaponMod()
                {
                    AccuracyPercentageModifier = 0,
                    CategoryId = "rangedWeaponMod",
                    ConflictingItemIds = Array.Empty<string>(),
                    ErgonomicsModifier = -6,
                    IconLink = "https://assets.tarkov.dev/61714eec290d254f5e6b2ffc-icon.jpg",
                    Id = "61714eec290d254f5e6b2ffc",
                    ImageLink = "https://assets.tarkov.dev/61714eec290d254f5e6b2ffc-image.jpg",
                    MarketLink = "https://tarkov.dev/item/schmidt-bender-pm-ii-3-12x50-scope",
                    Name = "Schmidt & Bender PM II 3-12x50 34mm riflescope",
                    RecoilPercentageModifier = 0,
                    ShortName = "PM II 3-12x50",
                    Weight = 0.9,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Schmidt_%26_Bender_PM_II_3-12x50_34mm_riflescope"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    Id = "testRangedWeaponMod",
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = 6,
                    IconLink = "https://assets.tarkov.dev/57e3dba62459770f0c32322b-icon.jpg",
                    Id = "57e3dba62459770f0c32322b",
                    ImageLink = "https://assets.tarkov.dev/57e3dba62459770f0c32322b-image.jpg",
                    MarketLink = "https://tarkov.dev/item/ak-bakelite-pistol-grip-6p4-sb9",
                    Name = "AK bakelite pistol grip (6P4 Sb.9)",
                    ShortName = "6P4 Sb.9",
                    Weight = 0.07,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AK_bakelite_pistol_grip_(6P4_Sb.9)"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = 10,
                    IconLink = "https://assets.tarkov.dev/57dc347d245977596754e7a1-icon.jpg",
                    Id = "57dc347d245977596754e7a1",
                    ImageLink = "https://assets.tarkov.dev/57dc347d245977596754e7a1-image.jpg",
                    MarketLink = "https://tarkov.dev/item/aks-74u-metal-skeleton-stock-6p26-sb5",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5a0c59791526d8dba737bba7"
                            },
                            Name = "mod_stock"
                        }
                    },
                    Name = "AKS-74U metal skeleton stock (6P26 Sb.5)",
                    RecoilPercentageModifier = -0.3,
                    ShortName = "6P26 Sb.5",
                    Weight = 0.218,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AKS-74U_metal_skeleton_stock_(6P26_Sb.5)"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = -2,
                    IconLink = "https://assets.tarkov.dev/57dc324a24597759501edc20-icon.jpg",
                    Id = "57dc324a24597759501edc20",
                    ImageLink = "https://assets.tarkov.dev/57dc324a24597759501edc20-image.jpg",
                    MarketLink = "https://tarkov.dev/item/aks-74u-545x39-muzzle-brake-6p26-0-20",
                    Name = "AKS-74U 5.45x39 muzzle brake (6P26 0-20)",
                    RecoilPercentageModifier = -0.08,
                    ShortName = "6P26 0-20",
                    Weight = 0.1,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AKS-74U_5.45x39_muzzle_brake_(6P26_0-20)"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = 5,
                    IconLink = "https://assets.tarkov.dev/57dc334d245977597164366f-icon.jpg",
                    Id = "57dc334d245977597164366f",
                    ImageLink = "https://assets.tarkov.dev/57dc334d245977597164366f-image.jpg",
                    MarketLink = "https://tarkov.dev/item/aks-74u-dust-cover-6p26-sb7",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "57ffb0062459777a045af529"
                            },
                            Name = "mod_mount_000"
                        }
                    },
                    Name = "AKS-74U dust cover (6P26 Sb.7)",
                    ShortName = "6P26 Sb.7",
                    Weight = 0.136,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AKS-74U_dust_cover_(6P26_Sb.7)"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    IconLink = "https://assets.tarkov.dev/59d36a0086f7747e673f3946-icon.jpg",
                    Id = "59d36a0086f7747e673f3946",
                    ImageLink = "https://assets.tarkov.dev/59d36a0086f7747e673f3946-image.jpg",
                    MarketLink = "https://tarkov.dev/item/aks-74u-gas-tube-6p26-sb1-2",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5d15ce51d7ad1a1eff619092",
                                "5a957c3fa2750c00137fa5f7",
                                "57dc32dc245977596d4ef3d3",
                                "57ffa9f4245977728561e844"
                            },
                            Name = "mod_handguard"
                        }
                    },
                    Name = "AKS-74U gas tube (6P26 Sb.1-2)",
                    ShortName = "6P26 Sb.1-2",
                    Weight = 0.03,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AKS-74U_gas_tube_(6P26_Sb.1-2)"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = 4,
                    IconLink = "https://assets.tarkov.dev/57dc32dc245977596d4ef3d3-icon.jpg",
                    Id = "57dc32dc245977596d4ef3d3",
                    ImageLink = "https://assets.tarkov.dev/57dc32dc245977596d4ef3d3-image.jpg",
                    MarketLink = "https://tarkov.dev/item/aks-74u-wooden-handguard-6p26-sb6",
                    Name = "AKS-74U wooden handguard (6P26 Sb.6)",
                    ShortName = "6P26 Sb.6",
                    Weight = 0.116,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AKS-74U_wooden_handguard_(6P26_Sb.6)"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = 9,
                    IconLink = "https://assets.tarkov.dev/57c55efc2459772d2c6271e7-icon.jpg",
                    Id = "57c55efc2459772d2c6271e7",
                    ImageLink = "https://assets.tarkov.dev/57c55efc2459772d2c6271e7-image.jpg",
                    MarketLink = "https://tarkov.dev/item/hogue-overmolded-rubber-grip-black",
                    Name = "Hogue OverMolded Rubber Grip (Black)",
                    ShortName = "OMRG BLK",
                    Weight = 0.08,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Hogue_OverMolded_Rubber_Grip"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = 5,
                    IconLink = "https://assets.tarkov.dev/5894a5b586f77426d2590767-icon.jpg",
                    Id = "5894a5b586f77426d2590767",
                    ImageLink = "https://assets.tarkov.dev/5894a5b586f77426d2590767-image.jpg",
                    MarketLink = "https://tarkov.dev/item/mpx-gen1-9x19-upper-receiver",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "57ac965c24597706be5f975c",
                                "57aca93d2459771f2c7e26db",
                                "544a3a774bdc2d3a388b4567",
                                "5d2dc3e548f035404a1a4798",
                                "57adff4f24597737f373b6e6",
                                "5c0517910db83400232ffee5",
                                "591c4efa86f7741030027726",
                                "570fd79bd2720bc7458b4583",
                                "570fd6c2d2720bc6458b457f",
                                "558022b54bdc2dac148b458d",
                                "5c07dd120db834001c39092d",
                                "5c0a2cec0db834001b7ce47d",
                                "58491f3324597764bc48fa02",
                                "584924ec24597768f12ae244",
                                "5b30b0dc5acfc400153b7124",
                                "6165ac8c290d254f5e6b2f6c",
                                "60a23797a37c940de7062d02",
                                "5d2da1e948f035477b1ce2ba",
                                "5c0505e00db834001b735073",
                                "609a63b6e2ff132951242d09",
                                "584984812459776a704a82a6",
                                "59f9d81586f7744c7506ee62",
                                "570fd721d2720bc5458b4596",
                                "57ae0171245977343c27bfcf",
                                "5dfe6104585a0c3e995c7b82",
                                "5d1b5e94d7ad1a2b865a96b0",
                                "609bab8b455afd752b2e6138",
                                "58d39d3d86f77445bb794ae7",
                                "616554fe50224f204c1da2aa",
                                "5c7d55f52e221644f31bff6a",
                                "616584766ef05c2ce828ef57",
                                "5b3b6dc75acfc47a8773fb1e",
                                "615d8d878004cc50514c3233",
                                "5b2389515acfc4771e1be0c0",
                                "577d128124597739d65d0e56",
                                "618b9643526131765025ab35",
                                "618bab21526131765025ab3f",
                                "5c86592b2e2216000e69e77c",
                                "5a37ca54c4a282000d72296a",
                                "5d0a29fed7ad1a002769ad08",
                                "58d2664f86f7747fec5834f6",
                                "57c69dd424597774c03b7bbc",
                                "5b3b99265acfc4704b4a1afb",
                                "5aa66a9be5b5b0214e506e89",
                                "5aa66c72e5b5b00016327c93",
                                "5c1cdd302e221602b3137250",
                                "61714b2467085e45ef140b2c",
                                "6171407e50224f204c1da3c5",
                                "61713cc4d8e3106d9806c109",
                                "5b31163c5acfc400153b71cb",
                                "5a33b652c4a28232996e407c",
                                "5a33b2c9c4a282000c5a9511",
                                "59db7eed86f77461f8380365",
                                "5a1ead28fcdbcb001912fa9f",
                                "5dff77c759400025ea5150cf",
                                "626bb8532c923541184624b4",
                                "62811f461d5df4475f46a332"
                            },
                            Name = "mod_scope"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5c5db5852e2216003a0fe71a",
                                "5c5db5962e2216000e5e46eb",
                                "58aeaaa886f7744fc1560f81",
                                "5894a2c386f77427140b8342",
                                "5c5db5b82e2216003a0fe71d",
                                "5c5db5c62e22160012542255"
                            },
                            Name = "mod_barrel"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5c59529a2e221602b177d160",
                                "5c5db6302e2216000e5e47f0",
                                "5c5db63a2e2216000f1b284a",
                                "5c5db5f22e2216000e5e47e8",
                                "5c5db5fc2e2216000f1b2842",
                                "5894a42086f77426d2590762"
                            },
                            Name = "mod_handguard"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5ba26b17d4351e00367f9bdd",
                                "5dfa3d7ac41b2312ea33362a",
                                "5c1780312e221602b66cc189",
                                "5fb6564947ce63734e3fa1da",
                                "5bc09a18d4351e003562b68e",
                                "5c18b9192e2216398b5a8104",
                                "5fc0fa957283c4046c58147e",
                                "5894a81786f77427140b8347"
                            },
                            Name = "mod_sight_rear"
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "57fd23e32459772d0805bcf1",
                                "544909bb4bdc2d6f028b4577",
                                "5c06595c0db834001a66af6c",
                                "5a7b483fe899ef0016170d15",
                                "61605d88ffa6e502ac5e7eeb",
                                "5c5952732e2216398b5abda2"
                            },
                            Name = "mod_tactical_000"
                        }
                    },
                    Name = "MPX GEN1 9x19 upper receiver",
                    ShortName = "MPX GEN1",
                    Weight = 0.488,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MPX_GEN1_9x19_upper_receiver"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = -2,
                    IconLink = "https://assets.tarkov.dev/57adff4f24597737f373b6e6-icon.jpg",
                    Id = "57adff4f24597737f373b6e6",
                    ImageLink = "https://assets.tarkov.dev/57adff4f24597737f373b6e6-image.jpg",
                    MarketLink = "https://tarkov.dev/item/sig-sauer-bravo4-4x30-scope",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "57ae0171245977343c27bfcf",
                                "58d39d3d86f77445bb794ae7",
                                "577d128124597739d65d0e56",
                                "58d2664f86f7747fec5834f6",
                                "5a33b2c9c4a282000c5a9511"
                            },
                            Name = "mod_scope"
                        }
                    },
                    Name = "SIG Sauer BRAVO4 4x30 scope",
                    ShortName = "BRAVO4",
                    Weight = 0.419,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/SIG_Sauer_BRAVO4_4x30_scope"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    IconLink = "https://assets.tarkov.dev/58d268fc86f774111273f8c2-icon.jpg",
                    Id = "58d268fc86f774111273f8c2",
                    ImageLink = "https://assets.tarkov.dev/58d268fc86f774111273f8c2-image.jpg",
                    MarketLink = "https://tarkov.dev/item/leupold-deltapoint-reflex-sight",
                    Name = "Leupold DeltaPoint Reflex Sight",
                    ShortName = "DP",
                    Weight = 0.053,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Leupold_DeltaPoint_Reflex_Sight"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = -3,
                    IconLink = "https://assets.tarkov.dev/58aeaaa886f7744fc1560f81-icon.jpg",
                    Id = "58aeaaa886f7744fc1560f81",
                    ImageLink = "https://assets.tarkov.dev/58aeaaa886f7744fc1560f81-image.jpg",
                    MarketLink = "https://tarkov.dev/item/mpx-sd-9x19-165mm-barrel",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "58aeac1b86f77457c419f475"
                            },
                            Name = "mod_muzzle",
                        }
                    },
                    Name = "MPX-SD 9x19 165mm barrel",
                    RecoilPercentageModifier = -0.02,
                    ShortName = "MPXSD 165mm",
                    Weight = 0.18,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MPX-SD_9x19_165mm_barrel"
                },
                new RangedWeaponMod()
                {
                    AccuracyPercentageModifier = -0.01,
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = -5,
                    IconLink = "https://assets.tarkov.dev/58aeac1b86f77457c419f475-icon.jpg",
                    Id = "58aeac1b86f77457c419f475",
                    ImageLink = "https://assets.tarkov.dev/58aeac1b86f77457c419f475-image.jpg",
                    MarketLink = "https://tarkov.dev/item/mpx-sd-9x19-integrated-sound-suppressor",
                    Name = "MPX-SD 9x19 integrated sound suppressor",
                    RecoilPercentageModifier = -0.15,
                    ShortName = "MPX-SD",
                    Weight = 0.6,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MPX-SD_9x19_integrated_sound_suppressor"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = 7,
                    IconLink = "https://assets.tarkov.dev/5894a42086f77426d2590762-icon.jpg",
                    Id = "5894a42086f77426d2590762",
                    ImageLink = "https://assets.tarkov.dev/5894a42086f77426d2590762-image.jpg",
                    MarketLink = "https://tarkov.dev/item/mpx-gen1-handguard",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "5ba26b01d4351e0085325a51",
                                "5dfa3d950dee1b22f862eae0",
                                "5c17804b2e2216152006c02f",
                                "5fb6567747ce63734e3fa1dc",
                                "5bc09a30d4351e00367fb7c8",
                                "5c18b90d2e2216152142466b",
                                "5fc0fa362770a0045c59c677",
                                "5894a73486f77426d259076c"
                            },
                            Name = "mod_sight_front",
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "58a56f8d86f774651579314c"
                            },
                            Name = "mod_mount_000",
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "58a5c12e86f7745d585a2b9e"
                            },
                            Name = "mod_mount_001",
                        },
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "58a56f8d86f774651579314c"
                            },
                            Name = "mod_mount_002",
                        }
                    },
                    Name = "MPX GEN1 handguard",
                    ShortName = "MPX GEN1",
                    Weight = 0.302,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MPX_GEN1_handguard"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = -1,
                    IconLink = "https://assets.tarkov.dev/5a7b483fe899ef0016170d15-icon.jpg",
                    Id = "5a7b483fe899ef0016170d15",
                    ImageLink = "https://assets.tarkov.dev/5a7b483fe899ef0016170d15-image.jpg",
                    MarketLink = "https://tarkov.dev/item/surefire-xc1-tactical-flashlight",
                    Name = "SureFire XC1 tactical flashlight",
                    ShortName = "XC1",
                    Weight = 0.05,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/SureFire_XC1_tactical_flashlight"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = 8,
                    IconLink = "https://assets.tarkov.dev/59f8a37386f7747af3328f06-icon.jpg",
                    Id = "59f8a37386f7747af3328f06",
                    ImageLink = "https://assets.tarkov.dev/59f8a37386f7747af3328f06-image.jpg",
                    MarketLink = "https://tarkov.dev/item/fortis-shift-tactical-foregrip",
                    Name = "Fortis Shift tactical foregrip",
                    RecoilPercentageModifier = -0.015,
                    ShortName = "Shift",
                    Weight = 0.09,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Fortis_Shift_tactical_foregrip"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = -2,
                    IconLink = "https://assets.tarkov.dev/58ac1bf086f77420ed183f9f-icon.jpg",
                    Id = "58ac1bf086f77420ed183f9f",
                    ImageLink = "https://assets.tarkov.dev/58ac1bf086f77420ed183f9f-image.jpg",
                    MarketLink = "https://tarkov.dev/item/mpxmcx-retractable-stock-pipe-adapter",
                    ModSlots = new ModSlot[]
                    {
                        new ModSlot()
                        {
                            CompatibleItemIds = new string[]
                            {
                                "57ade1442459771557167e15",
                                "5a33ca0fc4a282000d72292f",
                                "5c0faeddd174af02a962601f",
                                "5649be884bdc2d79388b4577",
                                "5d120a10d7ad1a4e1026ba85",
                                "5b0800175acfc400153aebd4",
                                "5947e98b86f774778f1448bc",
                                "5947eab886f77475961d96c5",
                                "5ef1ba28c64c5d0dfc0571a5",
                                "602e3f1254072b51b239f713",
                                "5c793fb92e221644f31bfb64",
                                "5c793fc42e221600114ca25d",
                                "591aef7986f774139d495f03",
                                "591af10186f774139d495f0e",
                                "627254cc9c563e6e442c398f",
                                "638de3603a1a4031d8260b8c"
                            },
                            Name = "mod_stock"
                        }
                    },
                    Name = "MPX/MCX retractable stock pipe adapter",
                    ShortName = "MPX/MCX",
                    Weight = 0.2,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MPX/MCX_retractable_stock_pipe_adapter"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = 18,
                    IconLink = "https://assets.tarkov.dev/591aef7986f774139d495f03-icon.jpg",
                    Id = "591aef7986f774139d495f03",
                    ImageLink = "https://assets.tarkov.dev/591aef7986f774139d495f03-image.jpg",
                    MarketLink = "https://tarkov.dev/item/troy-m7a1-pdw-stock-black",
                    Name = "TROY M7A1 PDW stock (Black)",
                    RecoilPercentageModifier = -0.26,
                    ShortName = "M7A1PDW",
                    Weight = 0.4,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/TROY_M7A1_PDW_stock"
                },
                new RangedWeaponMod()
                {
                    CategoryId = "rangedWeaponMod",
                    ErgonomicsModifier = 1,
                    IconLink = "https://assets.tarkov.dev/58949edd86f77409483e16a9-icon.jpg",
                    Id = "58949edd86f77409483e16a9",
                    ImageLink = "https://assets.tarkov.dev/58949edd86f77409483e16a9-image.jpg",
                    MarketLink = "https://tarkov.dev/item/mpx-double-latch-charging-handle",
                    Name = "MPX double latch charging handle",
                    ShortName = "MPX 2x",
                    Weight = 0.033,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MPX_double_latch_charging_handle"
                },
                new Vest()
                {
                    ArmorClass = 4,
                    ArmoredAreas = new string[]
                    {
                        "Thorax",
                        "Stomach"
                    },
                    Capacity = 12,
                    CategoryId = "vest",
                    Durability = 40,
                    ErgonomicsPercentageModifier = -0.15,
                    IconLink = "https://assets.tarkov.dev/5d5d646386f7742797261fd9-icon.jpg",
                    Id = "5d5d646386f7742797261fd9",
                    ImageLink = "https://assets.tarkov.dev/5d5d646386f7742797261fd9-image.jpg",
                    MarketLink = "https://tarkov.dev/item/6b3tm-01m-armored-rig",
                    Material = "Titan",
                    MovementSpeedPercentageModifier = -0.10,
                    Name = "6B3TM-01M armored rig",
                    ShortName = "6B3TM-01M",
                    TurningSpeedPercentageModifier = -0.05,
                    Weight = 9.2,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/6B3TM-01M_armored_rig"
                },
                new Vest()
                {
                    Capacity = 6,
                    CategoryId = "vest",
                    IconLink = "https://assets.tarkov.dev/572b7adb24597762ae139821-icon.jpg",
                    Id = "572b7adb24597762ae139821",
                    ImageLink = "https://assets.tarkov.dev/572b7adb24597762ae139821-image.jpg",
                    MarketLink = "https://tarkov.dev/item/scav-vest",
                    Name = "Scav Vest",
                    ShortName = "Scav Vest",
                    Weight = 0.4,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Scav_Vest"
                },
                new Vest()
                {
                    CategoryId = "vest",
                    Id = "testVest",
                }
            };
            result.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiItemsQuery = "{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
      },
      {
        ""categories"": [
          {
            ""id"": ""5c164d2286f774194c5e69fa""
          },
          {
            ""id"": ""543be5e94bdc2df1348b4568""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg"",
        ""id"": ""5c1d0c5f86f7744bb2683cf0"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg"",
        ""link"": ""https://tarkov.dev/item/terragroup-labs-keycard-blue"",
        ""name"": ""TerraGroup Labs keycard (Blue)"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesKey""
        },
        ""shortName"": ""Blue"",
        ""weight"": 0.01,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)""
      }
    ]
  }
}") }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<Item>>(It.IsAny<DataType>())).Returns(value: null);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>?>(TestData.ItemMissingProperties));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>?>(TestData.ArmorPenetrations));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<TarkovValues?>(TestData.TarkovValues));

            ItemsFetcher fetcher = new(
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object,
                armorPenetrationsFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(new Item[]
            {
                new Item()
                {
                    CategoryId = "other",
                    ConflictingItemIds = Array.Empty<string>(), // TODO : MISSING FROM API
                    IconLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg",
                    Id = "5c1d0c5f86f7744bb2683cf0",
                    ImageLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg",
                    MarketLink = "https://tarkov.dev/item/terragroup-labs-keycard-blue",
                    MaxStackableAmount = 1,
                    Name = "TerraGroup Labs keycard (Blue)",
                    ShortName = "Blue",
                    Weight = 0.01,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)"
                }
            }, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithNotImplementedItemCategory_ShouldIgnoreData()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiItemsQuery = "{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
        ""categories"": [
          {
            ""id"": ""NotImplementedItemType""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg"",
        ""id"": ""5c1d0c5f86f7744bb2683cf0"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg"",
        ""link"": ""https://tarkov.dev/item/terragroup-labs-keycard-blue"",
        ""name"": ""TerraGroup Labs keycard (Blue)"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesKey""
        },
        ""shortName"": ""Blue"",
        ""weight"": 0.01,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)""
      }
    ]
  }
}") }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(new ItemCategory[]
            {
                new ItemCategory()
                {
                    Id = "NotImplementedItemCategory",
                    Types = new ItemType[]
                    {
                        new ItemType()
                        {
                            Id = "NotImplementedItemType",
                            Name = "Not implemented item type"
                        }
                    }
                }
            }));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>?>(TestData.ItemMissingProperties));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>?>(TestData.ArmorPenetrations));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<TarkovValues?>(TestData.TarkovValues));

            ItemsFetcher fetcher = new(
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object,
                armorPenetrationsFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Fetch_WithoutItemCategories_ShouldReturnItemsWithOtherItemCategory()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiItemsQuery = "{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
        ""categories"": [
          {
            ""id"": ""5448e53e4bdc2d60728b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-icon.jpg"",
        ""id"": ""5ab8ebf186f7742d8b372e80"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-image.jpg"",
        ""link"": ""https://tarkov.dev/item/sso-attack-2-raid-backpack"",
        ""name"": ""SSO Attack 2 raid backpack"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesBackpack"",
          ""capacity"": 35
        },
        ""shortName"": ""Attack 2"",
        ""weight"": 6.12,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/SSO_Attack_2_raid_backpack""
      }
    ]
  }
}") }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(null));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>?>(TestData.ItemMissingProperties));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>?>(TestData.ArmorPenetrations));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<TarkovValues?>(TestData.TarkovValues));

            ItemsFetcher fetcher = new(
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object,
                armorPenetrationsFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(new Item[]
            {
                new Container()
                {
                    Capacity = 35,
                    CategoryId = "other",
                    IconLink = "https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-icon.jpg",
                    Id = "5ab8ebf186f7742d8b372e80",
                    ImageLink = "https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-image.jpg",
                    MarketLink = "https://tarkov.dev/item/sso-attack-2-raid-backpack",
                    Name = "SSO Attack 2 raid backpack",
                    ShortName = "Attack 2",
                    Weight = 6.12,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/SSO_Attack_2_raid_backpack"
                }
            });
        }
    }
}
