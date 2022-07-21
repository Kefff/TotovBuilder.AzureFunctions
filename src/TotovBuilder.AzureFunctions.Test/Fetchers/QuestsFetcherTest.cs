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
    /// Represents tests on the <see cref="QuestsFetcher"/> class.
    /// </summary>
    public class QuestsFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnQuests()
        {
            // Arrange
            Mock<ILogger<QuestsFetcher>> loggerMock = new Mock<ILogger<QuestsFetcher>>();

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiQuestsQueryKey)).Returns("{ tasks { id name wikiLink trader { normalizedName } } }");
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ApiUrlKey)).Returns("https://localhost/api");
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.FetchTimeoutKey)).Returns(5);

            Mock<IHttpClientWrapper> httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(TestData.QuestsJson) }));
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            QuestsFetcher fetcher = new QuestsFetcher(loggerMock.Object, httpClientWrapperFactoryMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            Quest[]? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Quests);
        }
    }
}
