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
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>>(TestData.ItemMissingProperties));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>>(TestData.ArmorPenetrations));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(TestData.TarkovValues));

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

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>>(Array.Empty<ItemMissingProperties>()));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>>(Array.Empty<ArmorPenetration>()));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(new TarkovValues()));

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
            Item[] expected = TestData.ItemsWithoutMissingProperties;
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

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>>(TestData.ItemMissingProperties));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>>(TestData.ArmorPenetrations));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(TestData.TarkovValues));

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
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>>(new ItemCategory[]
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
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>>(TestData.ItemMissingProperties));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>>(TestData.ArmorPenetrations));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(TestData.TarkovValues));

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

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>>(TestData.ItemMissingProperties));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>>(TestData.ArmorPenetrations));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(TestData.TarkovValues));

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

        [Fact]
        public async Task Fetch_WithInvalidPreset_ShouldReturnOnlyValidData()
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
              },
              {
                ""categories"": [
                  {
                    ""id"": ""57bef4c42459772e8d35a53b""
                  },
                  {
                    ""id"": ""543be5f84bdc2dd4348b456a""
                  },
                  {
                    ""id"": ""566162e44bdc2d3f298b4573""
                  },
                  {
                    ""id"": ""54009119af1c881c07000029""
                  }
                ],
                ""iconLink"": ""https://assets.tarkov.dev/preset-with-non-existing-base-item-icon.jpg"",
                ""id"": ""preset-with-non-existing-base-item"",
                ""inspectImageLink"": ""https://assets.tarkov.dev/preset-with-non-existing-base-item-image.jpg"",
                ""link"": ""https://tarkov.dev/item/preset-with-non-existing-base-item"",
                ""name"": ""Non existing base item"",
                ""properties"": {
                  ""__typename"": ""ItemPropertiesPreset"",
                  ""baseItem"": {
                    ""id"": ""non-existing-base-item""
                  },
                  ""moa"": null
                },
                ""shortName"": ""NEBI"",
                ""weight"": 0.01,
                ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/preset-with-non-existing-base-item""
              }
            ]
          }
        }") }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>>(TestData.ItemMissingProperties));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>>(TestData.ArmorPenetrations));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(TestData.TarkovValues));

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
        public async Task Fetch_WithPresetWithNotModdableBaseItem_ShouldReturnItemForThisPreset()
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
                ""iconLink"": ""https://assets.tarkov.dev/preset-with-non-moddable-base-item-icon.jpg"",
                ""id"": ""preset-with-non-moddable-base-item"",
                ""inspectImageLink"": ""https://assets.tarkov.dev/preset-with-non-moddable-base-item-image.jpg"",
                ""link"": ""https://tarkov.dev/item/preset-with-non-moddable-base-item"",
                ""name"": ""Non moddable base item"",
                ""properties"": {
                  ""__typename"": ""ItemPropertiesPreset"",
                  ""baseItem"": {
                    ""id"": ""5c1d0c5f86f7744bb2683cf0""
                  },
                  ""moa"": null
                },
                ""shortName"": ""NMBI"",
                ""weight"": 0.01,
                ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/preset-with-non-moddable-base-item""
              }
            ]
          }
        }") }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>>(TestData.ItemCategories));

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcher = new();
            itemMissingPropertiesFetcher.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemMissingProperties>>(TestData.ItemMissingProperties));

            Mock<IArmorPenetrationsFetcher> armorPenetrationsFetcherMock = new();
            armorPenetrationsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ArmorPenetration>>(TestData.ArmorPenetrations));

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(TestData.TarkovValues));

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
                    IconLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg",
                    Id = "5c1d0c5f86f7744bb2683cf0",
                    ImageLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg",
                    MarketLink = "https://tarkov.dev/item/terragroup-labs-keycard-blue",
                    MaxStackableAmount = 1,
                    Name = "TerraGroup Labs keycard (Blue)",
                    ShortName = "Blue",
                    Weight = 0.01,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)"
                },
                new Item()
                {
                    CategoryId = "other",
                    IconLink = "https://assets.tarkov.dev/preset-with-non-moddable-base-item-icon.jpg",
                    Id = "preset-with-non-moddable-base-item",
                    ImageLink = "https://assets.tarkov.dev/preset-with-non-moddable-base-item-image.jpg",
                    MarketLink = "https://tarkov.dev/item/preset-with-non-moddable-base-item",
                    MaxStackableAmount = 1,
                    Name = "Non moddable base item",
                    ShortName = "NMBI",
                    Weight = 0.01,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/preset-with-non-moddable-base-item"
                }
            }, options => options.RespectingRuntimeTypes());
        }
    }
}
