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
using TotovBuilder.Model.Abstractions.Items;
using TotovBuilder.Model.Builds;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="PFetcher"/> class.
    /// </summary>
    public class PFetcherTest
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
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PJson) };
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<IItemsFetcher> itemsFetcherMock = new();
            itemsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Item>?>(hasItems ? TestData.Items : null));

            PFetcher fetcher = new(
                new Mock<ILogger<PFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemsFetcherMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(hasItems ? TestData.P : Array.Empty<InventoryItem>());
        }

        [Fact]
        public async Task Fetch_WithNonModdableBaseItem_ShouldThrow()
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
              ""id"": ""non-moddable-item""
            },
            ""quantity"": 1
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/preset-non-moddable-item-icon.jpg"",
        ""id"": ""preset-non-moddable-item"",
        ""inspectImageLink"": ""https://assets.tarkov.dev/preset-non-moddable-item-image.jpg"",
        ""link"": ""https://tarkov.dev/item/preset-non-moddable-item"",
        ""name"": ""Face shield alone"",
        ""properties"": {
          ""baseItem"": {
            ""id"": ""non-moddable-item""
          },
          ""moa"": null
        },
        ""shortName"": ""FSA"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/preset-non-moddable-item""
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
            itemsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Item>?>(new Item[]
            {
                new NotSupportedItem()
                {
                    Id = "non-moddable-item"
                }
            }));

            PFetcher fetcher = new(
                new Mock<ILogger<PFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                itemsFetcherMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(Array.Empty<InventoryItem>());
        }

        private class NotSupportedItem : Item, IModdable
        {
            public string? DefaultPresetId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public ModSlot[] ModSlots { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
    }
}
