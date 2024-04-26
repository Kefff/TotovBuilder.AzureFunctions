using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
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
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiBartersQuery = "{ barters { level } }",
                ApiUrl = "https://localhost/api",
                ExecutionTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.BartersJson) }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            BartersFetcher fetcher = new BartersFetcher(
                new Mock<ILogger<BartersFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<Price>> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.Barters);
        }

        [Fact]
        public async Task Fetch_WithBartersRequiringObtainedItem_ShouldIgnoreThoseBarters()
        {
            // Arrange
            string bartersJson = @"{
  ""data"": {
    ""barters"": [
      {
        ""level"": 3,
        ""requiredItems"": [
          {
            ""item"": {
              ""id"": ""545cdb794bdc2d3a198b456a""
            },
            ""quantity"": 2
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
          ""normalizedName"": ""mechanic""
        }
      },
      {
        ""level"": 2,
        ""requiredItems"": [
          {
            ""item"": {
              ""id"": ""5448be9a4bdc2dfd2f8b456a""
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
          ""normalizedName"": ""mechanic""
        }
      }
    ]
  }
}";

            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiBartersQuery = "{ barters { level } }",
                ApiUrl = "https://localhost/api",
                ExecutionTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(bartersJson) }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            BartersFetcher fetcher = new BartersFetcher(
                new Mock<ILogger<BartersFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<Price>> result = await fetcher.Fetch();

            // Assert            
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(
                new Price[]
                {
                    new Price()
                    {
                        BarterItems = new BarterItem[]
                        {
                            new BarterItem()
                            {
                                ItemId = "5448be9a4bdc2dfd2f8b456a",
                                Quantity = 1
                            }
                        },
                        CurrencyName = "barter",
                        ItemId = "545cdb794bdc2d3a198b456a",
                        Merchant = "mechanic",
                        MerchantLevel = 2
                    }
                });
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiBartersQuery = "{ barters { level } }",
                ApiUrl = "https://localhost/api",
                ExecutionTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
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

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            BartersFetcher fetcher = new BartersFetcher(
                new Mock<ILogger<BartersFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<Price>> result = await fetcher.Fetch();

            // Assert            
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(new Price[]
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
