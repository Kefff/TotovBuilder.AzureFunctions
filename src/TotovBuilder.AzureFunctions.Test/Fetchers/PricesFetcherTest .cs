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
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using TotovBuilder.Model.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="PricesFetcher"/> class.
    /// </summary>
    public class PricesFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnPrices()
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
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PricesJson) }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            bartersFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.Barters)
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

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            List<Price> expected =
            [
                .. TestData.Prices.Concat(TestData.Barters),
                new()
                {
                    CurrencyName = "RUB",
                    ItemId = "5449016a4bdc2d6f028b456f",
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Value = 1,
                    ValueInMainCurrency = 1
                }
            ];

            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Fetch_WithBartersOnlyUsingCurrency_ShouldTransformBarterToPrice()
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
        ""buyFor"": [
          {
            ""currency"": ""RUB"",
            ""price"": 10000,
            ""priceRUB"": 10000,
            ""vendor"": {
              ""minTraderLevel"": 1,
              ""taskUnlock"": null,
              ""trader"": {
                ""normalizedName"": ""ref""
              }
            }
          }
        ],
        ""id"": ""5d235b4d86f7742e017bc88a""
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

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            bartersFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns([new()
                    {
                        BarterItems =
                        [
                            new BarterItem()
                            {
                                ItemId = "5d235b4d86f7742e017bc88a",
                                Quantity = 6
                            }
                        ],
                        CurrencyName = "barter",
                        ItemId = "67f90180f07898267b0a4ed7",
                        Merchant = "ref",
                        MerchantLevel = 2
                    }])
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

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            List<Price> expected =
            [
                new()
                {
                    CurrencyName = "GPCOIN",
                    ItemId = "67f90180f07898267b0a4ed7",
                    Merchant = "ref",
                    MerchantLevel = 2,
                    Value = 6,
                    ValueInMainCurrency = 60000
                },
                new()
                {
                    CurrencyName = "RUB",
                    ItemId = "5d235b4d86f7742e017bc88a",
                    Merchant = "ref",
                    MerchantLevel = 1,
                    Value = 10000,
                    ValueInMainCurrency = 10000
                },
                new Price()
                {
                    CurrencyName = "RUB",
                    ItemId = "5449016a4bdc2d6f028b456f",
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Value = 1,
                    ValueInMainCurrency = 1
                }
            ];

            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(expected);
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
        ""invalid"": {}
      },
      {
        ""buyFor"": [
          {
            ""currency"": ""RUB"",
            ""price"": 18111,
            ""priceRUB"": 18111,
            ""vendor"": {
              ""normalizedName"": ""flea-market""
            }
          }
        ],
        ""id"": ""5783c43d2459774bbe137486""
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

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            bartersFetcherMock
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

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new Price[]
            {
                new()
                {
                    CurrencyName = "RUB",
                    ItemId = "5783c43d2459774bbe137486",
                    Merchant = "flea-market",
                    Value = 18111,
                    ValueInMainCurrency = 18111
                },
                new()
                {
                    CurrencyName = "RUB",
                    ItemId = "5449016a4bdc2d6f028b456f",
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Value = 1,
                    ValueInMainCurrency = 1
                }
            });
        }

        [Fact]
        public async Task Fetch_WithFailedTarkovValuesFetch_ShouldFail()
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
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PricesJson) }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Barters - No data fetched.")))
                .Verifiable();

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Prices - No data fetched.");
        }

        [Fact]
        public async Task Fetch_WithFailedBartersFetch_ShouldReturnOnlyPrices()
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
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PricesJson) }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
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

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new List<Price>(TestData.Prices)
            {
                new()
                {
                    CurrencyName = "RUB",
                    ItemId = "5449016a4bdc2d6f028b456f",
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Value = 1,
                    ValueInMainCurrency = 1
                }
            });
        }
    }
}
