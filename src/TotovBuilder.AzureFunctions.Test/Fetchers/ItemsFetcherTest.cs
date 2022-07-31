using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Models.Items;
using TotovBuilder.AzureFunctions.Test.Mocks;
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
            Mock<ILogger<ItemsFetcher>> loggerMock = new Mock<ILogger<ItemsFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
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

            ItemsFetcher fetcher = new ItemsFetcher(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
                cacheMock.Object,
                new ItemCategoryFinder(itemCategoriesFetcherMock.Object));

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Items, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_ShouldIgnoreInvalidData()
        {
            // Arrange
            Mock<ILogger<ItemsFetcher>> loggerMock = new Mock<ILogger<ItemsFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
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

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            ItemsFetcher fetcher = new ItemsFetcher(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
                cacheMock.Object,
                new ItemCategoryFinder(itemCategoriesFetcherMock.Object));

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(new Item[]
            {
                new Item()
                {
                    CategoryId = "other",
                    //ConflictingItemIds = , // TODO : MISSING
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

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
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

            ItemsFetcher fetcher = new ItemsFetcher(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
                cacheMock.Object,
                new ItemCategoryFinder(itemCategoriesFetcherMock.Object));

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEmpty();
        }
    }
}
