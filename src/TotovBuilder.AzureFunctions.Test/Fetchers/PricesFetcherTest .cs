using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Items;
using Xunit;
using TotovBuilder.Model.Test;
using TotovBuilder.Model.Configuration;

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
            Mock<ILogger<PricesFetcher>> loggerMock = new Mock<ILogger<PricesFetcher>>();

            Mock<IAzureFunctionsConfigurationWrapper> azureFunctionsConfigurationWrapperMock = new Mock<IAzureFunctionsConfigurationWrapper>();
            azureFunctionsConfigurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiPricesQuery = "{ items(type: any) { id name buyFor { vendor { ... on TraderOffer { trader { normalizedName } minTraderLevel taskUnlock { id } } ... on FleaMarket { normalizedName } } price currency priceRUB } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PricesJson) }));
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            PricesFetcher fetcher = new PricesFetcher(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationWrapperMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Price>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Prices);
        }

        [Fact]
        public async void Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<ILogger<PricesFetcher>> loggerMock = new Mock<ILogger<PricesFetcher>>();

            Mock<IAzureFunctionsConfigurationWrapper> azureFunctionsConfigurationWrapperMock = new Mock<IAzureFunctionsConfigurationWrapper>();
            azureFunctionsConfigurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiPricesQuery = "{ items(type: any) { id name buyFor { vendor { ... on TraderOffer { trader { normalizedName } minTraderLevel taskUnlock { id } } ... on FleaMarket { normalizedName } } price currency priceRUB } } }",
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

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<Item>>(It.IsAny<DataType>())).Returns(value: null);

            PricesFetcher fetcher = new PricesFetcher(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationWrapperMock.Object, cacheMock.Object);

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
                }
            });
        }
    }
}
