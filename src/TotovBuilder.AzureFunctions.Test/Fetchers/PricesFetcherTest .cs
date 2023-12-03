using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
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
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiPricesQuery = "{ items { id name buyFor { vendor { ... on TraderOffer { trader { normalizedName } minTraderLevel taskUnlock { id } } ... on FleaMarket { normalizedName } } price currency priceRUB } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PricesJson) }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new Mock<ITarkovValuesFetcher>();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(Result.Ok(TestData.TarkovValues)));

            PricesFetcher fetcher = new PricesFetcher(
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result<IEnumerable<Price>> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(new List<Price>(TestData.Prices)
            {
                new Price()
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
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiPricesQuery = "{ items { id name buyFor { vendor { ... on TraderOffer { trader { normalizedName } minTraderLevel taskUnlock { id } } ... on FleaMarket { normalizedName } } price currency priceRUB } } }",
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
}") }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new Mock<ITarkovValuesFetcher>();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(Result.Ok(TestData.TarkovValues)));

            PricesFetcher fetcher = new PricesFetcher(
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            Result<IEnumerable<Price>> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(new Price[]
            {
                new Price()
                {
                    CurrencyName = "RUB",
                    ItemId = "5783c43d2459774bbe137486",
                    Merchant = "flea-market",
                    Value = 18111,
                    ValueInMainCurrency = 18111
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
            });
        }
    }
}
