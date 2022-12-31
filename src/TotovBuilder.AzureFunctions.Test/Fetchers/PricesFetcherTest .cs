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
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiPricesQuery = "{ items { id name buyFor { vendor { ... on TraderOffer { trader { normalizedName } minTraderLevel taskUnlock { id } } ... on FleaMarket { normalizedName } } price currency priceRUB } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PricesJson) }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<TarkovValues?>(TestData.TarkovValues));

            PricesFetcher fetcher = new(
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            IEnumerable<Price>? result = await fetcher.Fetch();

            // Assert
            List<Price> expected = new(TestData.Prices)
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
            };
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiPricesQuery = "{ items { id name buyFor { vendor { ... on TraderOffer { trader { normalizedName } minTraderLevel taskUnlock { id } } ... on FleaMarket { normalizedName } } price currency priceRUB } } }",
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

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<Item>>(It.IsAny<DataType>())).Returns(value: null);

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<TarkovValues?>(TestData.TarkovValues));

            PricesFetcher fetcher = new(
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object,
                tarkovValuesFetcherMock.Object);

            // Act
            IEnumerable<Price>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(new Price[]
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
