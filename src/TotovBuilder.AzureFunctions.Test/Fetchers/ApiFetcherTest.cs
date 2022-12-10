using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using Xunit;
using static System.Text.Json.JsonElement;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="ApiFetcher"/> class.
    /// </summary>
    public class ApiFetcherTest
    {
        [Fact]
        public async Task Fetch_WithPreviousFetchingTask_ShouldWaitForItToEndAndReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ items { id buyFor { currency price priceRUB vendor { ... on TraderOffer { minTraderLevel taskUnlock { id name wikiLink } trader { normalizedName }  } ... on FleaMarket { normalizedName }  }  }  }  }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PricesJson) };
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>())).Returns(TestData.Prices);

            ApiFetcherImplementation apiFetcher = new(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            _ = apiFetcher.Fetch();
            IEnumerable<Price>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Prices);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<IEnumerable<Price>>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithValidCachedData_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ items { id buyFor { currency price priceRUB vendor { ... on TraderOffer { minTraderLevel taskUnlock { id name wikiLink } trader { normalizedName }  } ... on FleaMarket { normalizedName }  }  }  }  }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(true);
            cacheMock.Setup(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>())).Returns(TestData.Prices);

            ApiFetcherImplementation apiFetcher = new(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Price>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Prices);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Never);
            cacheMock.Verify(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithoutValidCachedData_ShouldFetchDataAndCacheIt()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ items { id buyFor { currency price priceRUB vendor { ... on TraderOffer { minTraderLevel taskUnlock { id name wikiLink } trader { normalizedName }  } ... on FleaMarket { normalizedName }  }  }  }  }",
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

            ApiFetcherImplementation apiFetcher = new(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Price>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Prices);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>()), Times.Never);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<IEnumerable<Price>>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithInvalidConfiguration_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();
            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ tasks { id name trader { normalizedName } wikiLink } }"
            });

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>())).Returns(TestData.Prices);

            ApiFetcherImplementation apiFetcher = new(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Price>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Prices);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Never);
            cacheMock.Verify(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithTimeout_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ items { id buyFor { currency price priceRUB vendor { ... on TraderOffer { minTraderLevel taskUnlock { id name wikiLink } trader { normalizedName }  } ... on FleaMarket { normalizedName }  }  }  }  }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 1
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(2000);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>())).Returns(TestData.Prices);

            ApiFetcherImplementation apiFetcher = new(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Price>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Prices);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithError_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ items { id buyFor { currency price priceRUB vendor { ... on TraderOffer { minTraderLevel taskUnlock { id name wikiLink } trader { normalizedName }  } ... on FleaMarket { normalizedName }  }  }  }  }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Throws<Exception>();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>())).Returns(TestData.Prices);

            ApiFetcherImplementation apiFetcher = new(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Price>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Prices);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(TestData.EmptyApiData1)]
        [InlineData(TestData.EmptyApiData2)]
        [InlineData(TestData.EmptyApiData3)]
        [InlineData(TestData.InvalidApiData1)]
        [InlineData(TestData.InvalidApiData2)]
        [InlineData(TestData.InvalidApiData3)]
        public async Task Fetch_WithInvalidData_ShouldFail(string apiResponseData)
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ items { id buyFor { currency price priceRUB vendor { ... on TraderOffer { minTraderLevel taskUnlock { id name wikiLink } trader { normalizedName }  } ... on FleaMarket { normalizedName }  }  }  }  }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(apiResponseData) }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>())).Returns(() => null);

            ApiFetcherImplementation apiFetcher = new(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Price>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeNull();
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<Price>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<IEnumerable<Price>>()), Times.Never);
        }

        [Theory]
        [InlineData("{ \"value\": [\"123456789\"] }", true)]
        [InlineData("[\"123456789\"]", false)]
        public void TryDeserializeArray_ShouldTryToDeserializeArray(string json, bool expected)
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            Mock<ICache> cacheMock = new();

            ApiFetcherImplementation apiFetcher = new(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
                cacheMock.Object);

            JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

            // Act
            bool result = apiFetcher.TestTryDeserializeArray(jsonElement, "value", out ArrayEnumerator arrayEnumerator);

            // Assert
            result.Should().Be(expected);

            if (result)
            {
                arrayEnumerator.First().GetString().Should().Be("123456789");
            }
        }

        [Theory]
        [InlineData("{ \"value\": 123456789 }", true)]
        [InlineData("{ \"value\": \"123456789\"}", false)]
        [InlineData("{ \"invalid\": 123456789 }", false)]
        [InlineData("[123456789]", false)]
        public void TryDeserializeDouble_ShouldTryToDeserializeDouble(string json, bool expected)
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            Mock<ICache> cacheMock = new();

            ApiFetcherImplementation apiFetcher = new(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
                cacheMock.Object);

            JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

            // Act
            bool result = apiFetcher.TestTryDeserializeDouble(jsonElement, "value", out double resultValue);

            // Assert
            result.Should().Be(expected);

            if (result)
            {
                resultValue.Should().Be(123456789);
            }
        }

        [Theory]
        [InlineData("{ \"value\": { \"id\": \"123456789\" } }", true)]
        [InlineData("{ \"value\": \"123456789\" }", false)]
        [InlineData("{ \"invalid\": { \"id\": \"123456789\" } }", false)]
        [InlineData("[{ \"id\": \"123456789\" }]", false)]
        public void TryDeserializeObject_ShouldTryToDeserializeObject(string json, bool expected)
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            Mock<ICache> cacheMock = new();

            ApiFetcherImplementation apiFetcher = new(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
                cacheMock.Object);

            JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

            // Act
            bool result = apiFetcher.TestTryDeserializeObject(jsonElement, "value", out JsonElement resultValue);

            // Assert
            result.Should().Be(expected);

            if (result)
            {
                resultValue.GetProperty("id").GetString().Should().Be("123456789");
            }
        }

        [Theory]
        [InlineData("{ \"value\": \"123456789\" }", true)]
        [InlineData("{ \"value\": 123456789 }", false)]
        [InlineData("{ \"invalid\": \"123456789\" }", false)]
        [InlineData("[\"123456789\"]", false)]
        public void TryDeserializeString_ShouldTryToDeserializeString(string json, bool expected)
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new();
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            Mock<ICache> cacheMock = new();

            ApiFetcherImplementation apiFetcher = new(
                loggerMock.Object,
                httpClientWrapperFactoryMock.Object,
                azureFunctionsConfigurationReaderMock.Object,
                cacheMock.Object);

            JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

            // Act
            bool result = apiFetcher.TestTryDeserializeString(jsonElement, "value", out string resultValue);

            // Assert
            result.Should().Be(expected);

            if (result)
            {
                resultValue.Should().Be("123456789");
            }
        }

        public class ApiFetcherImplementation : ApiFetcher<IEnumerable<Price>>
        {
            protected override string ApiQuery => AzureFunctionsConfigurationReader.Values.ApiQuestsQuery;

            protected override DataType DataType => DataType.Prices;

            public ApiFetcherImplementation(ILogger<ApiFetcherImplementation> logger, IHttpClientWrapperFactory httpClientWrapperFactory, IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, ICache cache)
               : base(logger, httpClientWrapperFactory, azureFunctionsConfigurationReader, cache)
            {
            }

            public bool TestTryDeserializeArray(JsonElement jsonElement, string propertyName, out ArrayEnumerator value)
            {
                return TryDeserializeArray(jsonElement, propertyName, out value);
            }

            public bool TestTryDeserializeDouble(JsonElement jsonElement, string propertyName, out double value)
            {
                return TryDeserializeDouble(jsonElement, propertyName, out value);
            }

            public bool TestTryDeserializeObject(JsonElement jsonElement, string propertyName, out JsonElement value)
            {
                return TryDeserializeObject(jsonElement, propertyName, out value);
            }

            public bool TestTryDeserializeString(JsonElement jsonElement, string propertyName, out string value)
            {
                return TryDeserializeString(jsonElement, propertyName, out value);
            }

            protected override Task<Result<IEnumerable<Price>>> DeserializeData(string responseContent)
            {
                return Task.FromResult(Result.Ok(TestData.Prices.AsEnumerable()));
            }
        }
    }
}
