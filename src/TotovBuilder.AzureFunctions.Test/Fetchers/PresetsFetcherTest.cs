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
using TotovBuilder.Model.Abstractions.Items;
using TotovBuilder.Model.Builds;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="PresetsFetcher"/> class.
    /// </summary>
    public class PresetsFetcherTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Fetch_ShouldReturnPresets(bool hasItems)
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiPresetsQuery = "{ items(type: preset) { id properties { ... on ItemPropertiesPreset { baseItem { id } moa } } containsItems { item { id } quantity } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PresetsJson) };
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemsFetcher> itemsFetcherMock = new();
            itemsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Item>?>(hasItems ? TestData.Items : null));

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemsFetcherMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = (await fetcher.Fetch())?.OrderBy(p => p.ItemId);

            // Assert
            IEnumerable<InventoryItem> expected = TestData.Presets.OrderBy(i => i.ItemId);
            result.Should().BeEquivalentTo(hasItems ? expected : Array.Empty<InventoryItem>());
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiPresetsQuery = "{ items(type: preset) { id properties { ... on ItemPropertiesPreset { baseItem { id } moa } } containsItems { item { id } quantity } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
      },
      {
        ""containsItems"": [
            {
            ""item"": {
                ""id"": ""not-supported-item""
            },
            ""quantity"": 1
            }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/preset-not-supported-item-icon.jpg"",
        ""id"": ""preset-not-supported-item"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/preset-not-supported-item-image.jpg"",
        ""link"": ""https://tarkov.dev/item/preset-not-supported-item"",
        ""name"": ""Not moddable item"",
        ""properties"": {
            ""baseItem"": {
            ""id"": ""not-supported-item""
            },
            ""moa"": null
        },
        ""shortName"": ""NSI"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/preset-not-supported-item""
      },
      {
        ""containsItems"": [
          {
            ""item"": {
              ""id"": ""5a16b7e1fcdbcb00165aa6c9""
            },
            ""quantity"": 1
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/preset-item-without-properties-icon.jpg"",
        ""id"": ""preset-item-without-properties"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/preset-item-without-properties-image.jpg"",
        ""link"": ""https://tarkov.dev/item/preset-item-without-properties"",
        ""name"": ""Item without properties"",
        ""properties"": {
          ""baseItem"": {
            ""id"": ""item-without-properties""
          },
          ""moa"": null
        },
        ""shortName"": ""IWP"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/preset-item-without-properties""
      },
      {
        ""containsItems"": [
            {
            ""item"": {
                ""id"": ""5a16b7e1fcdbcb00165aa6c9""
            },
            ""quantity"": 1
            }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/preset-face-shield-alone-icon.jpg"",
        ""id"": ""preset-face-shield-alone"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/preset-face-shield-alone-image.jpg"",
        ""link"": ""https://tarkov.dev/item/preset-face-shield-alone"",
        ""name"": ""Face shield alone"",
        ""properties"": {
            ""baseItem"": {
            ""id"": ""5a16b7e1fcdbcb00165aa6c9""
            },
            ""moa"": null
        },
        ""shortName"": ""FSA"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/preset-face-shield-alone""
      }
    ]
  }
}") };
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemsFetcher> itemsFetcherMock = new();
            itemsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Item>?>(new List<Item>(TestData.Items)
            {
                new NotSupportedItem()
                {
                    Id = "not-supported-item"
                }
            }));

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemsFetcherMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(new InventoryItem[]
                {
                    new InventoryItem()
                    {
                        ItemId = "preset-face-shield-alone"
                    }
                });
        }

        [Fact]
        public async Task Fetch_WithIncompatibleAmmunitionInMagazine_ShouldTryFindingASlotUntilMaximumTriesAndReturnNothing()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiPresetsQuery = "{ items(type: preset) { id properties { ... on ItemPropertiesPreset { baseItem { id } moa } } containsItems { item { id } quantity } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
        ""containsItems"": [
            {
            ""item"": {
                ""id"": ""601aa3d2b2bcb34913271e6d""
            },
            ""quantity"": 30
            }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/preset-magazine-with-incompatible-ammunition.jpg"",
        ""id"": ""preset-magazine-with-incompatible-ammunition"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/preset-magazine-with-incompatible-ammunition.jpg"",
        ""link"": ""https://tarkov.dev/item/preset-magazine-with-incompatible-ammunition"",
        ""name"": ""Magazine with incompatible ammunition"",
        ""properties"": {
            ""baseItem"": {
            ""id"": ""564ca99c4bdc2d16268b4589""
            },
            ""moa"": null
        },
        ""shortName"": ""MWIA"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/preset-magazine-with-incompatible-ammunition""
      }
    ]
  }
}") };
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemsFetcher> itemsFetcherMock = new();
            itemsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Item>?>(TestData.Items));

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemsFetcherMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Fetch_WithNonMagazineItemContainingAmmunition_ShouldTryFindingASlotUntilMaximumTriesAndReturnNothing()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiPresetsQuery = "{ items(type: preset) { id properties { ... on ItemPropertiesPreset { baseItem { id } moa } } containsItems { item { id } quantity } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""items"": [
      {
        ""containsItems"": [
            {
            ""item"": {
                ""id"": ""601aa3d2b2bcb34913271e6d""
            },
            ""quantity"": 30
            }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/preset-non-magazine-item-with-ammunition.jpg"",
        ""id"": ""preset-non-magazine-item-with-ammunition"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/preset-non-magazine-item-with-ammunition.jpg"",
        ""link"": ""https://tarkov.dev/item/preset-non-magazine-item-with-ammunition"",
        ""name"": ""Non magazine with ammunition"",
        ""properties"": {
            ""baseItem"": {
            ""id"": ""57dc324a24597759501edc20""
            },
            ""moa"": null
        },
        ""shortName"": ""NMWA"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/preset-non-magazine-item-with-ammunition""
      }
    ]
  }
}") };
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemsFetcher> itemsFetcherMock = new();
            itemsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Item>?>(TestData.Items));

            PresetsFetcher fetcher = new(
                new Mock<ILogger<PresetsFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemsFetcherMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEmpty();
        }

        private class NotSupportedItem : Item, IModdable
        {
            public string? DefaultPresetId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public bool IsPreset { get; set; }
            public ModSlot[] ModSlots { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
    }
}
