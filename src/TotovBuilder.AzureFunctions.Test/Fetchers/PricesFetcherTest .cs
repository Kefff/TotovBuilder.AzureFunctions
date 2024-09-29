﻿using System.Collections.Generic;
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
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
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
                .Returns(new AzureFunctionsConfiguration()
                {
                    ApiPresetsQuery = "{ items(type: preset) { id } }",
                    ApiPricesQuery = "{ items { id }",
                    ApiUrl = "https://localhost/api",
                    ExecutionTimeout = 5
                })
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
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new List<Price>(TestData.Prices.Concat(TestData.Barters))
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
            bartersFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(new AzureFunctionsConfiguration()
                {
                    ApiPricesQuery = "{ items { id }",
                    ApiUrl = "https://localhost/api",
                    ExecutionTimeout = 5
                })
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
            bartersFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
        }

        [Fact]
        public async Task Fetch_WithFailedTarkovValuesFetch_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(new AzureFunctionsConfiguration()
                {
                    ApiPricesQuery = "{ items { id }",
                    ApiUrl = "https://localhost/api",
                    ExecutionTimeout = 5
                })
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
            tarkovValuesFetcherMock.Verify();
        }

        [Fact]
        public async Task Fetch_WithFailedBartersFetch_ShouldReturnOnlyPrices()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(new AzureFunctionsConfiguration()
                {
                    ApiPricesQuery = "{ items { id name buyFor { vendor { ... on TraderOffer { trader { normalizedName } minTraderLevel taskUnlock { id } } ... on FleaMarket { normalizedName } } price currency priceRUB } } }",
                    ApiUrl = "https://localhost/api",
                    ExecutionTimeout = 5
                })
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
            bartersFetcherMock.Verify();
            tarkovValuesFetcherMock.Verify();
        }
    }
}
