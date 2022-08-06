using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model;
using Xunit;
using TotovBuilder.Model.Test;

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

            QuestsFetcher fetcher = new QuestsFetcher(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Quest>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Quests);
        }

        [Fact]
        public async void Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<ILogger<QuestsFetcher>> loggerMock = new Mock<ILogger<QuestsFetcher>>();

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
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{
  ""data"": {
    ""tasks"": [
      {
        ""invalid"": {}
      },
      {
        ""id"": ""59675d6c86f7740a842fc482"",
        ""name"": ""Ice Cream Cones"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Ice_Cream_Cones"",
        ""trader"": {
          ""normalizedName"": ""prapor""
        }
      }
    ]
  }
}") }));
            
            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            httpClientWrapperFactoryMock.Setup(m => m.Create()).Returns(httpClientWrapperMock.Object);

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<Quest>>(It.IsAny<DataType>())).Returns(value: null);

            QuestsFetcher fetcher = new QuestsFetcher(loggerMock.Object, httpClientWrapperFactoryMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<Quest>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(new Quest[]
            {
                new Quest()
                {
                    Id = "59675d6c86f7740a842fc482",
                    Name = "Ice Cream Cones",
                    Merchant = "prapor",
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Ice_Cream_Cones"
                }
            });
        }
    }
}
