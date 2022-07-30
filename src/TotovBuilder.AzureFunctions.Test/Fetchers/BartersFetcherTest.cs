using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
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
    /// Represents tests on the <see cref="BartersFetcher"/> class.
    /// </summary>
    public class BartersFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnBarters()
        {
            // Arrange
            Mock<ILogger<BartersFetcher>> loggerMock = new Mock<ILogger<BartersFetcher>>();

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiBartersQueryKey)).Returns("{ barters { level requiredItems { item { id } quantity } rewardItems { item { id } quantity } trader { normalizedName } taskUnlock { id }  }  }");
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiUrlKey)).Returns("https://localhost/api");
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.FetchTimeoutKey)).Returns(5);

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.BartersJson) }));
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            BartersFetcher fetcher = new BartersFetcher(loggerMock.Object, httpClientWrapperFactoryMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Item>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Barters);
        }
    }
}
