using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Models;
using TotovBuilder.AzureFunctions.Test.Mocks;
using Xunit;

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
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ tasks { id name wikiLink trader { normalizedName } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });
            azureFunctionsConfigurationReaderMock.Setup(m => m.WaitUntilReady()).Returns(Task.CompletedTask);
            
            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.QuestsJson) };
                });
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>())).Returns(TestData.Quests);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            _ = apiFetcher.Fetch();
            IEnumerable<Quest>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Quests);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<IEnumerable<Quest>>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithValidCachedData_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ tasks { id name wikiLink trader { normalizedName } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });
            
            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(true);
            cacheMock.Setup(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>())).Returns(TestData.Quests);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Quest>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Quests);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Never);
            cacheMock.Verify(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithoutValidCachedData_ShouldFetchDataAndCacheIt()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ tasks { id name wikiLink trader { normalizedName } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.QuestsJson) }));
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Quest>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Quests);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>()), Times.Never);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<IEnumerable<Quest>>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithInvalidConfiguration_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();
            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ tasks { id name trader { normalizedName } wikiLink } }"
            });
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>())).Returns(TestData.Quests);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Quest>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Quests);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Never);
            cacheMock.Verify(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithTimeout_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ tasks { id name wikiLink trader { normalizedName } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 1
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
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>())).Returns(TestData.Quests);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Quest>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Quests);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithError_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ tasks { id name wikiLink trader { normalizedName } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Throws<Exception>();
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>())).Returns(TestData.Quests);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Quest>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Quests);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>()), Times.Once);
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
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                ApiQuestsQuery = "{ tasks { id name wikiLink trader { normalizedName } } }",
                ApiUrl = "https://localhost/api",
                FetchTimeout = 5
            });

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(apiResponseData) }));
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>())).Returns(() => null);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Quest>? result = await apiFetcher.Fetch();

            // Assert
            result.Should().BeNull();
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<IEnumerable<Quest>>()), Times.Never);
        }

        public class ApiFetcherImplementation : ApiFetcher<IEnumerable<Quest>>
        {
            protected override string ApiQuery => AzureFunctionsConfigurationReader.Values.ApiQuestsQuery;

            protected override DataType DataType => DataType.Prices;

            public ApiFetcherImplementation(ILogger logger, IHttpClientWrapperFactory httpClientWrapperFactory, IAzureFunctionsConfigurationReader azureFunctionsConfigurationReader, ICache cache) 
               : base(logger, httpClientWrapperFactory, azureFunctionsConfigurationReader, cache)
            {
            }

            protected override Task<Result<IEnumerable<Quest>>> DeserializeData(string responseContent)
            {
                return Task.FromResult(Result.Ok(TestData.Quests.AsEnumerable()));
            }
        }
    }
}
