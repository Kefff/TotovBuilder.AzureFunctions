using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Test.Mocks;
using TotovBuilder.AzureFunctions.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Utils
{
    /// <summary>
    /// Represents tests on the <see cref="MarketDataFetcher"/> class.
    /// </summary>
    public class MarketDataFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldGetMarketData()
        {
            // Arrange
            Mock<ILogger<MarketDataFetcher>> logger = new Mock<ILogger<MarketDataFetcher>>();
            Mock<IConfigurationReader> configurationReaderMock = GetConfigurationReaderMock();

            SuccessHttpMessageHandler httpMessageHandler = new SuccessHttpMessageHandler();
            TestHttpClientFactory httpClientFactory = new TestHttpClientFactory(httpMessageHandler);

            MarketDataFetcher fetcher = new MarketDataFetcher(logger.Object, httpClientFactory, configurationReaderMock.Object);

            // Act
            Result<string> responseResult = await fetcher.Fetch();

            // Assert
            responseResult.IsSuccess.Should().BeTrue();
            responseResult.Value.Should().Be(TestData.MarketDataItemsOnly);
            httpMessageHandler.SentRequestProperties.Should().Be(@"Method: POST, RequestUri: 'https://test.com/graphql', Version: 1.1, Content: System.Net.Http.StringContent, Headers:
{
  Accept: application/json
  Content-Type: application/json; charset=utf-8
}");
            httpMessageHandler.SentRequestContent.Should().Be("{\"query\":\"{\\r\\n  itemsByType(type: ammo) {\\r\\n    id,\\r\\n    name,\\r\\n    normalizedName,\\r\\n    shortName,\\r\\n    iconLink,\\r\\n    wikiLink,\\r\\n    imageLink,\\r\\n    gridImageLink,\\r\\n    link,\\r\\n    buyFor {\\r\\n      source,\\r\\n      price,\\r\\n      currency,\\r\\n      requirements {\\r\\n        type,\\r\\n        value\\r\\n      }\\r\\n    }\\r\\n  }\\r\\n}\"}");
        }

        [Fact]
        public async Task Fetch_WithInvalidConfiguration_ShouldFail()
        {
            // Arrange
            Mock<ILogger<MarketDataFetcher>> logger = new Mock<ILogger<MarketDataFetcher>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiUrlKey)).Returns("");

            SuccessHttpMessageHandler httpMessageHandler = new SuccessHttpMessageHandler();
            TestHttpClientFactory httpClientFactory = new TestHttpClientFactory(httpMessageHandler);


            MarketDataFetcher fetcher = new MarketDataFetcher(logger.Object, httpClientFactory, configurationReaderMock.Object);

            // Act
            Result<string> responseResult = await fetcher.Fetch();

            // Assert
            responseResult.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Fetch_WithHttpError_ShouldFail()
        {
            // Arrange
            Mock<ILogger<MarketDataFetcher>> logger = new Mock<ILogger<MarketDataFetcher>>();
            Mock<IConfigurationReader> configurationReaderMock = GetConfigurationReaderMock();

            ExceptionHttpMessageHandler httpMessageHandler = new ExceptionHttpMessageHandler();
            TestHttpClientFactory httpClientFactory = new TestHttpClientFactory(httpMessageHandler);

            MarketDataFetcher fetcher = new MarketDataFetcher(logger.Object, httpClientFactory, configurationReaderMock.Object);

            // Act
            Result<string> responseResult = await fetcher.Fetch();

            // Assert
            responseResult.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Fetch_WithInvalidResponse_ShouldFail()
        {
            // Arrange
            Mock<ILogger<MarketDataFetcher>> logger = new Mock<ILogger<MarketDataFetcher>>();
            Mock<IConfigurationReader> configurationReaderMock = GetConfigurationReaderMock();

            InvalidHttpMessageHandler httpMessageHandler = new InvalidHttpMessageHandler();
            TestHttpClientFactory httpClientFactory = new TestHttpClientFactory(httpMessageHandler);

            MarketDataFetcher fetcher = new MarketDataFetcher(logger.Object, httpClientFactory, configurationReaderMock.Object);

            // Act
            Result<string> responseResult = await fetcher.Fetch();

            // Assert
            responseResult.IsSuccess.Should().BeFalse();
        }

        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Fetch_WithInvalidResponseData_ShouldFail(bool hasDataProperty)
        {
            // Arrange
            Mock<ILogger<MarketDataFetcher>> logger = new Mock<ILogger<MarketDataFetcher>>();
            Mock<IConfigurationReader> configurationReaderMock = GetConfigurationReaderMock();

            HttpMessageHandler httpMessageHandler;

            if (hasDataProperty)
            {
                httpMessageHandler = new InvalidDataHttpMessageHandler1();
            }
            else
            {
                httpMessageHandler = new InvalidDataHttpMessageHandler2();
            }

            TestHttpClientFactory httpClientFactory = new TestHttpClientFactory(httpMessageHandler);

            MarketDataFetcher fetcher = new MarketDataFetcher(logger.Object, httpClientFactory, configurationReaderMock.Object);

            // Act
            Result<string> responseResult = await fetcher.Fetch();

            // Assert
            responseResult.IsSuccess.Should().BeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Fetch_WithEmptyResponseData_ShouldFail(int emptyResponseType)
        {
            // Arrange
            Mock<ILogger<MarketDataFetcher>> logger = new Mock<ILogger<MarketDataFetcher>>();
            Mock<IConfigurationReader> configurationReaderMock = GetConfigurationReaderMock();

            TestHttpClientFactory httpClientFactory = emptyResponseType switch
            {
                1 => new TestHttpClientFactory(new EmptyResponseDataHttpMessageHandler1()),
                2 => new TestHttpClientFactory(new EmptyResponseDataHttpMessageHandler2()),
                3 => new TestHttpClientFactory(new EmptyResponseDataHttpMessageHandler3()),
                _ => throw new NotImplementedException(),
            };

            MarketDataFetcher fetcher = new MarketDataFetcher(logger.Object, httpClientFactory, configurationReaderMock.Object);

            // Act
            Result<string> responseResult = await fetcher.Fetch();

            // Assert
            responseResult.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Fetch_WithTimeout_ShouldFail()
        {
            // Arrange
            Mock<ILogger<MarketDataFetcher>> logger = new Mock<ILogger<MarketDataFetcher>>();
            Mock<IConfigurationReader> configurationReaderMock = GetConfigurationReaderMock();
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.FetchTimeoutKey)).Returns(1);

            DelayedHttpMessageHandler httpMessageHandler = new DelayedHttpMessageHandler(1500);
            TestHttpClientFactory httpClientFactory = new TestHttpClientFactory(httpMessageHandler);

            MarketDataFetcher fetcher = new MarketDataFetcher(logger.Object, httpClientFactory, configurationReaderMock.Object);

            // Act
            Result<string> responseResult = await fetcher.Fetch();

            // Assert
            responseResult.IsSuccess.Should().BeFalse();
        }

        /// <summary>
        /// Represents a test HTTP client factory.
        /// </summary>
        private class TestHttpClientFactory : IHttpClientFactory
        {
            /// <summary>
            /// HTTP message handler.
            /// </summary>
            private readonly HttpMessageHandler MessageHandler;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestHttpClientFactory"/> class.
            /// </summary>
            /// <param name="httpMessageHandler">HTTP message handler.</param>
            public TestHttpClientFactory(HttpMessageHandler httpMessageHandler)
            {
                MessageHandler = httpMessageHandler;
            }

            /// <inheritdoc/>
            public HttpClient CreateClient(string name)
            {
                return new HttpClient(MessageHandler);
            }
        }

        /// <summary>
        /// Represents a test HTTP message handler that throws an exception.
        /// </summary>
        private class ExceptionHttpMessageHandler : HttpMessageHandler
        {
            /// <inheritdoc/>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new Exception("HTTP error");
            }
        }

        /// <summary>
        /// Represents a test HTTP message handler that return an invalid response.
        /// </summary>
        private class InvalidHttpMessageHandler : HttpMessageHandler
        {
            /// <inheritdoc/>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }
        }

        /// <summary>
        /// Represents a test HTTP message handler that return success response with invalid data.
        /// </summary>
        private class InvalidDataHttpMessageHandler1 : HttpMessageHandler
        {
            /// <inheritdoc/>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(@"{
  ""data"": { }
}")
                });
            }
        }

        /// <summary>
        /// Represents a test HTTP message handler that return success response with invalid data.
        /// </summary>
        private class InvalidDataHttpMessageHandler2 : HttpMessageHandler
        {
            /// <inheritdoc/>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("{ }")
                });
            }
        }

        /// <summary>
        /// Represents a test HTTP message handler that sucessfully executes.
        /// </summary>
        private class SuccessHttpMessageHandler : HttpMessageHandler
        {
            /// <summary>
            /// Sent request.
            /// </summary>
            public string SentRequestProperties = string.Empty;

            /// <summary>
            /// Sent request.
            /// </summary>
            public string SentRequestContent = string.Empty;

            /// <inheritdoc/>
            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                SentRequestProperties = request.ToString();
                SentRequestContent = await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.MarketData) };
            }
        }

        /// <summary>
        /// Represents a test HTTP message handler that sucessfully executes but returns empty data.
        /// </summary>
        private class EmptyResponseDataHttpMessageHandler1 : HttpMessageHandler
        {
            /// <inheritdoc/>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.EmptyMarketData1) });
            }
        }

        /// <summary>
        /// Represents a test HTTP message handler that sucessfully executes but returns empty data.
        /// </summary>
        private class EmptyResponseDataHttpMessageHandler2 : HttpMessageHandler
        {
            /// <inheritdoc/>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.EmptyMarketData2) });
            }
        }

        /// <summary>
        /// Represents a test HTTP message handler that sucessfully executes but returns empty data.
        /// </summary>
        private class EmptyResponseDataHttpMessageHandler3 : HttpMessageHandler
        {
            /// <inheritdoc/>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.EmptyMarketData3) });
            }
        }

        /// <summary>
        /// Represents a test HTTP message handler that executes sucessfully with a delay.
        /// </summary>
        private class DelayedHttpMessageHandler : HttpMessageHandler
        {
            private readonly int Delay;

            /// <summary>
            /// Initializes a new instance of the <see cref="DelayedHttpMessageHandler"/> class.
            /// </summary>
            /// <param name="delay">Delay in milliseconds..</param>
            public DelayedHttpMessageHandler(int delay)
            {
                Delay = delay;
            }

            /// <inheritdoc/>
            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                await Task.Delay(Delay);

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.MarketData) };
            }
        }

        /// <summary>
        /// Gets a configuration reader mock.
        /// </summary>
        /// <returns>Configuration reader mock.</returns>
        private Mock<IConfigurationReader> GetConfigurationReaderMock()
        {
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiPriceQueryKey)).Returns(@"{
  itemsByType(type: ammo) {
    id,
    name,
    normalizedName,
    shortName,
    iconLink,
    wikiLink,
    imageLink,
    gridImageLink,
    link,
    buyFor {
      source,
      price,
      currency,
      requirements {
        type,
        value
      }
    }
  }
}");
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiUrlKey)).Returns("https://test.com/graphql");

            return configurationReaderMock;
        }
    }
}
