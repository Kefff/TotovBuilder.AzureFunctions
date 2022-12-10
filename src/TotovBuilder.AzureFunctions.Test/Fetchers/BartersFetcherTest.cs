using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="BartersFetcher"/> class.
    /// </summary>
    public class BartersFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnBarters()
        {
            // Arrange
            Mock<ILogger<BartersFetcher>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiBartersQuery = "{ barters { level requiredItems { item { id } quantity } rewardItems { item { id } quantity } trader { normalizedName } taskUnlock { id }  }  }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.BartersJson) }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            BartersFetcher fetcher = new(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Price>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Barters);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<ILogger<BartersFetcher>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiBartersQuery = "{ barters { level requiredItems { item { id } quantity } rewardItems { item { id } quantity } trader { normalizedName } taskUnlock { id }  }  }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""barters"": [
      {
        ""invalid"": { }
      },
      {
        ""level"": 1,
        ""requiredItems"": [
          {
            ""item"": {
              ""id"": ""5e32f56fcb6d5863cc5e5ee4""
            },
            ""quantity"": 2
          },
          {
            ""item"": {
              ""id"": ""5b432be65acfc433000ed01f""
            },
            ""quantity"": 1
          }
        ],
        ""rewardItems"": [
          {
            ""item"": {
              ""id"": ""545cdb794bdc2d3a198b456a""
            },
            ""quantity"": 1
          }
        ],
        ""trader"": {
          ""normalizedName"": ""prapor""
        },
        ""taskUnlock"": {
          ""id"": ""59675d6c86f7740a842fc482"",
          ""name"": ""Ice Cream Cones"",
          ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Ice_Cream_Cones""
        }
      }
    ]
  }
}") }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<Item>>(It.IsAny<DataType>())).Returns(value: null);

            BartersFetcher fetcher = new(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Price>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(new Price[]
            {
                new Price()
                {
                    BarterItems = new BarterItem[]
                    {
                        new BarterItem()
                        {
                            ItemId = "5e32f56fcb6d5863cc5e5ee4",
                            Quantity = 2
                        },
                        new BarterItem()
                        {
                            ItemId = "5b432be65acfc433000ed01f",
                            Quantity = 1
                        }
                    },
                    CurrencyName = "barter",
                    ItemId = "545cdb794bdc2d3a198b456a",
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Quest = new Quest()
                    {
                        Id = "59675d6c86f7740a842fc482",
                        Name = "Ice Cream Cones",
                        WikiLink = "https://escapefromtarkov.fandom.com/wiki/Ice_Cream_Cones"
                    }
                }
            });
        }
    }
}
