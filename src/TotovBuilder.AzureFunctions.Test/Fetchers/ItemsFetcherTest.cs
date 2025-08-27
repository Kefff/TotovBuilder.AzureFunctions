using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Abstractions.Items;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using TotovBuilder.Model.Utils;
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
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.ItemsJson) }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
                .Verifiable();

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcherMock = new();
            itemMissingPropertiesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemMissingPropertiesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemMissingProperties)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            ItemsFetcher itemsFetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            Mock<ILocalizedItemsFetcher> localizedItemsFetcherMock = new();
            localizedItemsFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            localizedItemsFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(() => [
                    new LocalizedItems()
                    {
                        Items = [.. itemsFetcher.FetchedData!],
                        Language = "en"
                    }
                ]);

            PresetsFetcher presetsFetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                localizedItemsFetcherMock.Object);

            // Act
            Result itemsFetchResult = await itemsFetcher.Fetch();

            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PresetsJson) }))
                .Verifiable();

            Result presetsFetchResult = await presetsFetcher.Fetch(); // The PresetsFetcher updates item properties

            // Assert
            itemsFetchResult.IsSuccess.Should().BeTrue();
            itemsFetcher.FetchedData.Should().NotBeNull();
            presetsFetchResult.IsSuccess.Should().BeTrue();
            presetsFetcher.FetchedData.Should().NotBeNull();

            IEnumerable<IItem> orderedResult = localizedItemsFetcherMock.Object.FetchedData!
                .Single(fd => fd.Language == "en")
                .Items
                    .OrderBy(i => $"{i.CategoryId} - {i.Name}");
            IEnumerable<Item> expected = TestData.Items.OrderBy(i => $"{i.CategoryId} - {i.Name}");

            orderedResult.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithoutItemMissingProperties_ShouldReturnItems()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
        ""categories"": [
          {
            ""id"": ""543be5dd4bdc2deb348b4569""
          },
          {
            ""id"": ""5661632d4bdc2d903d8b456b""
          },
          {
            ""id"": ""54009119af1c881c07000029""
          }
        ],
        ""conflictingItems"": [],
        ""iconLink"": ""https://assets.tarkov.dev/569668774bdc2da2298b4568-icon.webp"",
        ""id"": ""569668774bdc2da2298b4568"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/569668774bdc2da2298b4568-image.webp"",
        ""link"": ""https://tarkov.dev/item/euros"",
        ""name"": ""Euros"",
        ""properties"": null,
        ""shortName"": ""EUR"",
        ""weight"": 0,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Euros""
      }
    ]
  }
}") }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
                .Verifiable();

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcherMock = new();
            itemMissingPropertiesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemMissingPropertiesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns([])
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();

            Item[] expected =
            [
                new Item()
                {
                    CategoryId = "currency",
                    IconLink = "https://assets.tarkov.dev/569668774bdc2da2298b4568-icon.webp",
                    Id = "569668774bdc2da2298b4568",
                    ImageLink = "https://assets.tarkov.dev/569668774bdc2da2298b4568-image.webp",
                    MarketLink = "https://tarkov.dev/item/euros",
                    MaxStackableAmount = 1,
                    Name = "Euros",
                    ShortName = "EUR",
                    Weight = 0,
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Euros"
                }
            ];

            fetcher.FetchedData.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

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
        }") }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
                .Verifiable();

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcherMock = new();
            itemMissingPropertiesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemMissingPropertiesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemMissingProperties)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new Item[]
            {
                new()
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
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

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
        }") }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(new ItemCategory[]
                {
                    new()
                    {
                        Id = "NotImplementedItemCategory",
                        Types =
                        [
                            new ItemType()
                            {
                                Id = "NotImplementedItemType",
                                Name = "Not implemented item type"
                            }
                        ]
                    }
                })
                .Verifiable();

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcherMock = new();
            itemMissingPropertiesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemMissingPropertiesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemMissingProperties)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEmpty();
        }

        [Fact]
        public async Task Fetch_WithFailedRawDataFetching_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

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
        }") }))
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcherMock = new();
            itemMissingPropertiesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemMissingPropertiesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemMissingProperties)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Items - No data fetched.");
        }

        [Fact]
        public async Task Fetch_WithInvalidPreset_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

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
        }") }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
                .Verifiable();

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcherMock = new();
            itemMissingPropertiesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemMissingPropertiesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemMissingProperties)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new Item[]
            {
                new()
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
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

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
        }") }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemCategoriesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemCategories)
                .Verifiable();

            Mock<IItemMissingPropertiesFetcher> itemMissingPropertiesFetcherMock = new();
            itemMissingPropertiesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            itemMissingPropertiesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.ItemMissingProperties)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            ItemsFetcher fetcher = new(
                "en",
                new Mock<ILogger<ItemsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                itemCategoriesFetcherMock.Object,
                itemMissingPropertiesFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new Item[]
            {
                new()
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
                new()
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
