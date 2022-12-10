using System;
using System.Collections.Generic;
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
            Mock<ILogger<ItemsFetcher>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
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
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
                cacheMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object,
                armorPenetrationsFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Items, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithoutItemMissingProperties_ShouldReturnItems()
        {
            // Arrange
            Mock<ILogger<ItemsFetcher>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
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
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
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
                    Deafening = string.Empty,
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
                new RangedWeapon()
                {
                    Caliber = "Caliber545x39",
                    CategoryId = "mainWeapon",
                    DefaultPresetId = "584147732459775a2b6d9f12",
                    Ergonomics = 44,
                    FireModes = new string[] { "SingleFire", "FullAuto" },
                    FireRate = 650,
                    HorizontalRecoil = 445,
                    IconLink = "https://assets.tarkov.dev/584147732459775a2b6d9f12-icon.jpg",
                    Id = "57dc2fa62459775949412633",
                    ImageLink = "https://assets.tarkov.dev/584147732459775a2b6d9f12-image.jpg",
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
                    DefaultPresetId = "5eb2968186f7746d1f1a4fd5",
                    Ergonomics = 75,
                    FireModes = new string[] { "SingleFire" },
                    FireRate = 30,
                    HorizontalRecoil = 355,
                    IconLink = "https://assets.tarkov.dev/5eb2968186f7746d1f1a4fd5-icon.jpg",
                    Id = "5e81c3cbac2bb513793cdc75",
                    ImageLink = "https://assets.tarkov.dev/5eb2968186f7746d1f1a4fd5-image.jpg",
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
                    Id = "testRangedWeapon",
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
            }, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<ILogger<ItemsFetcher>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
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
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
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
            Mock<ILogger<ItemsFetcher>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
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
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
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
            Mock<ILogger<ItemsFetcher>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
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
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
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
