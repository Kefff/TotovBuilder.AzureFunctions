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
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Utils;
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
        public async Task Fetch_ShouldFetchedData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiUrl = "https://localhost/api",
                ExecutionTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PricesJson) };
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<Price>> result = await apiFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.Prices);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithPreviousFetchingTask_ShouldWaitForItToEndAndReturnFetchedData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiUrl = "https://localhost/api",
                ExecutionTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PricesJson) };
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object);

            // Act
            _ = apiFetcher.Fetch();
            Result<IEnumerable<Price>> result = await apiFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.Prices);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_AlreadyFetchedData_ShouldReturnFetchedData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiUrl = "https://localhost/api",
                ExecutionTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.PricesJson) };
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object);

            // Act
            await apiFetcher.Fetch();
            Result<IEnumerable<Price>> result = await apiFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.Prices);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithInvalidConfiguration_ShouldFail()
        {
            // Arrange
            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();

            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration());

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                new Mock<IHttpClientWrapperFactory>().Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<Price>> result = await apiFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Prices - No data fetched.");
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithTimeout_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiUrl = "https://localhost/api",
                ExecutionTimeout = 1
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(2000);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<Price>> result = await apiFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Prices - No data fetched.");
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithError_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiUrl = "https://localhost/api",
                ExecutionTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Throws<Exception>();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<Price>> result = await apiFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Prices - No data fetched.");
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [Theory]
        [InlineData(TestData.EmptyApiData1)]
        [InlineData(TestData.EmptyApiData2)]
        [InlineData(TestData.EmptyApiData3)]
        [InlineData(TestData.InvalidApiData1)]
        [InlineData(TestData.InvalidApiData2)]
        [InlineData(TestData.InvalidApiData3)]
        public async Task Fetch_WithInvalidDataWithoutCache_ShouldFail(string apiResponseData)
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiUrl = "https://localhost/api",
                ExecutionTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(apiResponseData) }));

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<Price>> result = await apiFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Prices - No data fetched.");
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [Theory]
        [InlineData("{ \"value\": [\"123456789\"] }", true)]
        [InlineData("[\"123456789\"]", false)]
        public void TryDeserializeArray_ShouldTryToDeserializeArray(string json, bool expected)
        {
            // Arrange
            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                new Mock<IHttpClientWrapperFactory>().Object,
                new Mock<IConfigurationWrapper>().Object);

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
            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                new Mock<IHttpClientWrapperFactory>().Object,
                new Mock<IConfigurationWrapper>().Object);

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
            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                new Mock<IHttpClientWrapperFactory>().Object,
                new Mock<IConfigurationWrapper>().Object);

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
            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(
                new Mock<ILogger<ApiFetcherImplementation>>().Object,
                new Mock<IHttpClientWrapperFactory>().Object,
                new Mock<IConfigurationWrapper>().Object);

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
            protected override string ApiQuery
            {
                get
                {
                    return "{ items { id buyFor { currency price priceRUB vendor { ... on TraderOffer { minTraderLevel taskUnlock { id name wikiLink } trader { normalizedName } } ... on FleaMarket { normalizedName } } } } }";
                }
            }

            protected override DataType DataType
            {
                get
                {
                    return DataType.Prices;
                }
            }

            public ApiFetcherImplementation(
                ILogger<ApiFetcherImplementation> logger,
                IHttpClientWrapperFactory httpClientWrapperFactory,
                IConfigurationWrapper configurationWrapper)
               : base(logger, httpClientWrapperFactory, configurationWrapper)
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
