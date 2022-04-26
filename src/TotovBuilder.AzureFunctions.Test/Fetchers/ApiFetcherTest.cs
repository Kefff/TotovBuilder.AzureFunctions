using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Fetchers;
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

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiItemsMetadataQueryKey)).Returns("{ itemsByType(type: any) { id name iconLink wikiLink imageLink link } }");
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiUrlKey)).Returns("https://localhost/api");
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.FetchTimeoutKey)).Returns(5);
            
            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.ItemsMetadata) };
                });
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get(It.IsAny<DataType>())).Returns(TestData.ItemsMetadata);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            _ = apiFetcher.Fetch();
            string result = await apiFetcher.Fetch();

            // Assert
            result.Should().Be(TestData.ItemsMetadata);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithValidCachedData_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiItemsMetadataQueryKey)).Returns("{ itemsByType(type: any) { id name iconLink wikiLink imageLink link } }");
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiUrlKey)).Returns("https://localhost/api");
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.FetchTimeoutKey)).Returns(5);
            
            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(true);
            cacheMock.Setup(m => m.Get(It.IsAny<DataType>())).Returns(TestData.ItemsMetadata);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            string result = await apiFetcher.Fetch();

            // Assert
            result.Should().Be(TestData.ItemsMetadata);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Never);
            cacheMock.Verify(m => m.Get(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithoutValidCachedData_ShouldFetchDataAndCacheIt()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiItemsMetadataQueryKey)).Returns("{ itemsByType(type: any) { id name iconLink wikiLink imageLink link } }");
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiUrlKey)).Returns("https://localhost/api");
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.FetchTimeoutKey)).Returns(5);

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.ItemsMetadata) }));
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            string result = await apiFetcher.Fetch();

            // Assert
            result.Should().Be(TestData.ItemsMetadata);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get(It.IsAny<DataType>()), Times.Never);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithInvalidConfiguration_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get(It.IsAny<DataType>())).Returns(TestData.ItemsMetadata);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            string result = await apiFetcher.Fetch();

            // Assert
            result.Should().Be(TestData.ItemsMetadata);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Never);
            cacheMock.Verify(m => m.Get(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithTimeout_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiItemsMetadataQueryKey)).Returns("{ itemsByType(type: any) { id name iconLink wikiLink imageLink link } }");
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiUrlKey)).Returns("https://localhost/api");
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.FetchTimeoutKey)).Returns(1);

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
            cacheMock.Setup(m => m.Get(It.IsAny<DataType>())).Returns(TestData.ItemsMetadata);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            string result = await apiFetcher.Fetch();

            // Assert
            result.Should().Be(TestData.ItemsMetadata);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithError_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiItemsMetadataQueryKey)).Returns("{ itemsByType(type: any) { id name iconLink wikiLink imageLink link } }");
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiUrlKey)).Returns("https://localhost/api");
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.FetchTimeoutKey)).Returns(5);

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Throws<Exception>();
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get(It.IsAny<DataType>())).Returns(TestData.ItemsMetadata);

            ApiFetcherImplementation apiFetcher = new ApiFetcherImplementation(loggerMock.Object, httpClientWrapperFactoryMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            string result = await apiFetcher.Fetch();

            // Assert
            result.Should().Be(TestData.ItemsMetadata);
            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            cacheMock.Verify(m => m.Get(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }
    }

    public class ApiFetcherImplementation : ApiFetcher
    {
        protected override string ApiQueryKey => TotovBuilder.AzureFunctions.ConfigurationReader.ApiItemsMetadataQueryKey;

        protected override DataType DataType => DataType.ItemCategories;

        public ApiFetcherImplementation(ILogger logger, IHttpClientWrapperFactory httpClientWrapperFactory, IConfigurationReader configurationReader, ICache cache) 
           : base(logger, httpClientWrapperFactory, configurationReader, cache)
        {
        }

        protected override Result<string> GetData(string responseContent)
        {
            return Result.Ok(responseContent);
        }
    }
}
