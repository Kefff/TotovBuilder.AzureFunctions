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
using TotovBuilder.Model.Items;
using Xunit;
using TotovBuilder.Model;
using TotovBuilder.Model.Test;
using System;

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
            Mock<ILogger<ItemsFetcher>> loggerMock = new Mock<ILogger<ItemsFetcher>>();

            Mock<IAzureFunctionsConfigurationWrapper> azureFunctionsConfigurationWrapperMock = new Mock<IAzureFunctionsConfigurationWrapper>();
            azureFunctionsConfigurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiItemsQuery = "{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.ItemsJson) }));
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new Mock<IItemMissingPropertiesFetcher>();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>?>(TestData.ItemMissingProperties));

            ItemsFetcher fetcher = new ItemsFetcher(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationWrapperMock.Object,
                cacheMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object);

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Items, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithoutItemMissingProperties_ShouldReturnItems()
        {
            // Arrange
            Mock<ILogger<ItemsFetcher>> loggerMock = new Mock<ILogger<ItemsFetcher>>();

            Mock<IAzureFunctionsConfigurationWrapper> azureFunctionsConfigurationWrapperMock = new Mock<IAzureFunctionsConfigurationWrapper>();
            azureFunctionsConfigurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiItemsQuery = "{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
        ""categories"": [
          {
            ""id"": ""57bef4c42459772e8d35a53b""
          },
          {
            ""id"": ""543be5f84bdc2dd4348b456a""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5a16b7e1fcdbcb00165aa6c9-icon.jpg"",
        ""id"": ""5a16b7e1fcdbcb00165aa6c9"",
        ""imageLink"": ""https://assets.tarkov.dev/5a16b7e1fcdbcb00165aa6c9-image.jpg"",
        ""link"": ""https://tarkov.dev/item/ops-core-fast-multi-hit-ballistic-face-shield"",
        ""name"": ""Ops-Core FAST multi-hit ballistic face shield"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesArmorAttachment"",
          ""blindnessProtection"": 0.1,
          ""class"": 3,
          ""durability"": 40,
          ""ergoPenalty"": -8,
          ""headZones"": [""Eyes"", ""Jaws""],
          ""material"": {
            ""name"": ""Glass""
          },
          ""speedPenalty"": 0,
          ""turnPenalty"": -0.08
        },
        ""shortName"": ""FAST FS"",
        ""weight"": 1.2,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Ops-Core_FAST_multi-hit_ballistic_face_shield""
      },
      {
        ""categories"": [
          {
            ""id"": ""5a341c4086f77401f2541505""
          },
          {
            ""id"": ""57bef4c42459772e8d35a53b""
          },
          {
            ""id"": ""543be5f84bdc2dd4348b456a""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5e4bfc1586f774264f7582d3-icon.jpg"",
        ""id"": ""5e4bfc1586f774264f7582d3"",
        ""imageLink"": ""https://assets.tarkov.dev/5e4bfc1586f774264f7582d3-image.jpg"",
        ""link"": ""https://tarkov.dev/item/msa-gallet-tc-800-high-cut-combat-helmet"",
        ""name"": ""MSA Gallet TC 800 High Cut combat helmet"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesHelmet"",
          ""class"": 4,
          ""deafening"": ""None"",
          ""durability"": 30,
          ""ergoPenalty"": -6,
          ""headZones"": [""Top"", ""Nape""],
          ""material"": {
            ""name"": ""Combined materials""
          },
          ""speedPenalty"": -0.02,
          ""turnPenalty"": -0.08
        },
        ""shortName"": ""TC 800"",
        ""weight"": 1.17,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/MSA_Gallet_TC_800_High_Cut_combat_helmet""
      },
      {
        ""categories"": [
          {
            ""id"": ""543be5dd4bdc2deb348b4569""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/569668774bdc2da2298b4568-icon.jpg"",
        ""id"": ""569668774bdc2da2298b4568"",
        ""imageLink"": ""https://assets.tarkov.dev/569668774bdc2da2298b4568-image.jpg"",
        ""link"": ""https://tarkov.dev/item/euros"",
        ""name"": ""Euros"",
        ""properties"": null,
        ""shortName"": ""EUR"",
        ""weight"": 0,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Euros""
      },
      {
        ""categories"": [
          {
            ""id"": ""5448bc234bdc2d3c308b4569""
          },
          {
            ""id"": ""55802f3e4bdc2de7118b4584""
          },
          {
            ""id"": ""5448fe124bdc2da5018b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5e81c4ca763d9f754677befa-icon.jpg"",
        ""id"": ""5e81c4ca763d9f754677befa"",
        ""imageLink"": ""https://assets.tarkov.dev/5e81c4ca763d9f754677befa-image.jpg"",
        ""link"": ""https://tarkov.dev/item/m1911a1-45-acp-7-round-magazine"",
        ""name"": ""M1911A1 .45 ACP 7-round magazine"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesMagazine"",
          ""ammoCheckModifier"": -0.2,
          ""capacity"": 7,
          ""ergonomics"": -1,
          ""loadModifier"": -0.25,
          ""malfunctionChance"": 0.04
        },
        ""shortName"": ""1911"",
        ""weight"": 0.16,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/M1911A1_.45_ACP_7-round_magazine""
      },
      {
        ""categories"": [
          {
            ""id"": ""55818b224bdc2dde698b456f""
          },
          {
            ""id"": ""55802f3e4bdc2de7118b4584""
          },
          {
            ""id"": ""5448fe124bdc2da5018b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/57d17e212459775a1179a0f5-icon.jpg"",
        ""id"": ""57d17e212459775a1179a0f5"",
        ""imageLink"": ""https://assets.tarkov.dev/57d17e212459775a1179a0f5-image.jpg"",
        ""link"": ""https://tarkov.dev/item/kiba-arms-25mm-accessory-ring-mount"",
        ""name"": ""Kiba Arms 25mm accessory ring mount"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesWeaponMod"",
          ""accuracyModifier"": 0,
          ""ergonomics"": -1,
          ""recoilModifier"": 0
        },
        ""shortName"": ""25mm ring"",
        ""weight"": 0.085,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Kiba_Arms_25mm_accessory_ring_mount""
      },
      {
        ""categories"": [
          {
            ""id"": ""5447b5f14bdc2d61278b4567""
          },
          {
            ""id"": ""5422acb9af1c889c16000029""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/57dc2fa62459775949412633-icon.jpg"",
        ""id"": ""57dc2fa62459775949412633"",
        ""imageLink"": ""https://assets.tarkov.dev/57dc2fa62459775949412633-image.jpg"",
        ""link"": ""https://tarkov.dev/item/kalashnikov-aks-74u-545x39-assault-rifle"",
        ""name"": ""Kalashnikov AKS-74U 5.45x39 assault rifle"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesWeapon"",
          ""caliber"": ""Caliber545x39"",
          ""ergonomics"": 44,
          ""fireModes"": [""Single fire"", ""Full Auto""],
          ""fireRate"": 650,
          ""recoilHorizontal"": 445,
          ""recoilVertical"": 141
        },
        ""shortName"": ""AKS-74U"",
        ""weight"": 1.809,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Kalashnikov_AKS-74U_5.45x39_assault_rifle""
      },
      {
        ""categories"": [
          {
            ""id"": ""55818a304bdc2db5418b457d""
          },
          {
            ""id"": ""55802f4a4bdc2ddb688b4569""
          },
          {
            ""id"": ""5448fe124bdc2da5018b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5d2c76ed48f03532f2136169-icon.jpg"",
        ""id"": ""5d2c76ed48f03532f2136169"",
        ""imageLink"": ""https://assets.tarkov.dev/5d2c76ed48f03532f2136169-image.jpg"",
        ""link"": ""https://tarkov.dev/item/ak-akademia-bastion-dust-cover"",
        ""name"": ""AK AKademia Bastion dust cover"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesWeaponMod"",
          ""accuracyModifier"": 0,
          ""ergonomics"": 5,
          ""recoilModifier"": -0.01
        },
        ""shortName"": ""Bastion"",
        ""weight"": 0.237,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/AK_AKademia_Bastion_dust_cover""
      }
    ]
  }
}") }));
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<Item>>(It.IsAny<DataType>())).Returns(value: null);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new Mock<IItemMissingPropertiesFetcher>();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>?>(null));

            ItemsFetcher fetcher = new ItemsFetcher(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationWrapperMock.Object,
                cacheMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object);

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(new Item[]
            {
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
                    ConflictingItemIds = Array.Empty<string>(), // TODO : MISSING FROM API
                    Durability = 40,
                    ErgonomicsPercentageModifier = -0.08,
                    IconLink = "https://assets.tarkov.dev/5a16b7e1fcdbcb00165aa6c9-icon.jpg",
                    Id = "5a16b7e1fcdbcb00165aa6c9",
                    ImageLink = "https://assets.tarkov.dev/5a16b7e1fcdbcb00165aa6c9-image.jpg",
                    MarketLink = "https://tarkov.dev/item/ops-core-fast-multi-hit-ballistic-face-shield",
                    Material = "Glass",
                    MaxStackableAmount = 1,
                    ModSlots = Array.Empty<ModSlot>(), // TODO : MISSING FROM API
                    MovementSpeedPercentageModifier = 0,
                    Name = "Ops-Core FAST multi-hit ballistic face shield",
                    //RicochetChance = , // TODO : MISSING FROM API
                    ShortName = "FAST FS",
                    TurningSpeedPercentageModifier = -0.08,
                    Weight = 1.2,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Ops-Core_FAST_multi-hit_ballistic_face_shield",
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
                    ConflictingItemIds = Array.Empty<string>(), // TODO : MISSING FROM API
                    Deafening = "None",
                    Durability = 30,
                    ErgonomicsPercentageModifier = -0.06,
                    IconLink = "https://assets.tarkov.dev/5e4bfc1586f774264f7582d3-icon.jpg",
                    Id = "5e4bfc1586f774264f7582d3",
                    ImageLink = "https://assets.tarkov.dev/5e4bfc1586f774264f7582d3-image.jpg",
                    MarketLink = "https://tarkov.dev/item/msa-gallet-tc-800-high-cut-combat-helmet",
                    Material = "Combined materials",
                    MaxStackableAmount = 1,
                    ModSlots = Array.Empty<ModSlot>(), // TODO : MISSING FROM API
                    MovementSpeedPercentageModifier = -0.02,
                    Name = "MSA Gallet TC 800 High Cut combat helmet",
                    //RicochetChance = , // TODO : MISSING FROM API
                    ShortName = "TC 800",
                    TurningSpeedPercentageModifier = -0.08,
                    Weight = 1.17,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/MSA_Gallet_TC_800_High_Cut_combat_helmet"
                },
                new Item()
                {
                    CategoryId = "currency",
                    ConflictingItemIds = Array.Empty<string>(), // TODO : MISSING FROM API
                    IconLink = "https://assets.tarkov.dev/569668774bdc2da2298b4568-icon.jpg",
                    Id = "569668774bdc2da2298b4568",
                    ImageLink = "https://assets.tarkov.dev/569668774bdc2da2298b4568-image.jpg",
                    MarketLink = "https://tarkov.dev/item/euros",
                    MaxStackableAmount = 1, // TODO : MISSING FROM API
                    Name = "Euros",
                    ShortName = "EUR",
                    Weight = 0,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Euros"
                },
                new Magazine()
                {
                    AcceptedAmmunitionIds = Array.Empty<string>(), // TODO : MISSING FROM API
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
                    MaxStackableAmount = 1,
                    ModSlots = Array.Empty<ModSlot>(),
                    Name = "M1911A1 .45 ACP 7-round magazine",
                    ShortName = "1911",
                    Weight = 0.16,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/M1911A1_.45_ACP_7-round_magazine"
                },
                new Mod()
                {
                    CategoryId = "mod",
                    ConflictingItemIds = Array.Empty<string>(), // TODO : MISSING FROM API
                    ErgonomicsModifier = -1,
                    IconLink = "https://assets.tarkov.dev/57d17e212459775a1179a0f5-icon.jpg",
                    Id = "57d17e212459775a1179a0f5",
                    ImageLink = "https://assets.tarkov.dev/57d17e212459775a1179a0f5-image.jpg",
                    MarketLink = "https://tarkov.dev/item/kiba-arms-25mm-accessory-ring-mount",
                    ModSlots = Array.Empty<ModSlot>(), // TODO : MISSING FROM API
                    Name = "Kiba Arms 25mm accessory ring mount",
                    ShortName = "25mm ring",
                    Weight = 0.085,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Kiba_Arms_25mm_accessory_ring_mount"
                },
                new RangedWeapon()
                {
                    Caliber = "Caliber545x39",
                    CategoryId = "mainWeapon",
                    ConflictingItemIds = Array.Empty<string>(), // TODO : MISSING FROM API
                    Ergonomics = 44,
                    FireModes = new string[] { "Single fire", "Full auto" },
                    FireRate = 650,
                    HorizontalRecoil = 445,
                    IconLink = "https://assets.tarkov.dev/57dc2fa62459775949412633-icon.jpg",
                    Id = "57dc2fa62459775949412633",
                    ImageLink = "https://assets.tarkov.dev/57dc2fa62459775949412633-image.jpg",
                    MarketLink = "https://tarkov.dev/item/kalashnikov-aks-74u-545x39-assault-rifle",
                    MaxStackableAmount = 1,
                    ModSlots = Array.Empty<ModSlot>(), // TODO : MISSING FROM API
                    Name = "Kalashnikov AKS-74U 5.45x39 assault rifle",
                    ShortName = "AKS-74U",
                    VerticalRecoil = 141,
                    Weight = 1.809,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Kalashnikov_AKS-74U_5.45x39_assault_rifle"
                },
                new RangedWeaponMod()
                {
                    AccuracyPercentageModifier = 0,
                    CategoryId = "rangedWeaponMod",
                    ConflictingItemIds = Array.Empty<string>(), // TODO : MISSING FROM API
                    ErgonomicsModifier = 5,
                    IconLink = "https://assets.tarkov.dev/5d2c76ed48f03532f2136169-icon.jpg",
                    Id = "5d2c76ed48f03532f2136169",
                    ImageLink = "https://assets.tarkov.dev/5d2c76ed48f03532f2136169-image.jpg",
                    MarketLink = "https://tarkov.dev/item/ak-akademia-bastion-dust-cover",
                    MaxStackableAmount = 1,
                    ModSlots = Array.Empty<ModSlot>(), // TODO : MISSING FROM API
                    Name = "AK AKademia Bastion dust cover",
                    RecoilPercentageModifier = -0.01,
                    ShortName = "Bastion",
                    Weight = 0.237,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/AK_AKademia_Bastion_dust_cover"
                }
            }, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<ILogger<ItemsFetcher>> loggerMock = new Mock<ILogger<ItemsFetcher>>();

            Mock<IAzureFunctionsConfigurationWrapper> azureFunctionsConfigurationWrapperMock = new Mock<IAzureFunctionsConfigurationWrapper>();
            azureFunctionsConfigurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiItemsQuery = "{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
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
        ""imageLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg"",
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
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<Item>>(It.IsAny<DataType>())).Returns(value: null);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new Mock<IItemMissingPropertiesFetcher>();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>?>(TestData.ItemMissingProperties));

            ItemsFetcher fetcher = new ItemsFetcher(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationWrapperMock.Object,
                cacheMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object);

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
            Mock<ILogger<ItemsFetcher>> loggerMock = new Mock<ILogger<ItemsFetcher>>();

            Mock<IAzureFunctionsConfigurationWrapper> azureFunctionsConfigurationWrapperMock = new Mock<IAzureFunctionsConfigurationWrapper>();
            azureFunctionsConfigurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiItemsQuery = "{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
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
        ""imageLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg"",
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
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
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

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new Mock<IItemMissingPropertiesFetcher>();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>?>(TestData.ItemMissingProperties));

            ItemsFetcher fetcher = new ItemsFetcher(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationWrapperMock.Object,
                cacheMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcher.Object);

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEmpty();
        }
    }
}
