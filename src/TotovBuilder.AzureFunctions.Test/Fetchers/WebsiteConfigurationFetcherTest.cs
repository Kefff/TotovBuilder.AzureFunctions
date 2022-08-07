using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model;
using Xunit;
using TotovBuilder.Model.Test;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="WebsiteConfigurationFetcher"/> class.
    /// </summary>
    public class WebsiteConfigurationFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnWebsiteConfiguration()
        {
            // Arrange
            Mock<ILogger<WebsiteConfigurationFetcher>> loggerMock = new Mock<ILogger<WebsiteConfigurationFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureWebsiteConfigurationBlobName = "website-configuration.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.WebsiteConfigurationJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            WebsiteConfigurationFetcher fetcher = new WebsiteConfigurationFetcher(loggerMock.Object, blobDataFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            WebsiteConfiguration? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.WebsiteConfiguration);
        }

        [Fact]
        public async void Fetch_WithInvalidData_ShouldReturnNull()
        {
            // Arrange
            Mock<ILogger<WebsiteConfigurationFetcher>> loggerMock = new Mock<ILogger<WebsiteConfigurationFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureWebsiteConfigurationBlobName = "website-configuration.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"{
  invalid,
  ""bugReportUrl"": ""https://discord.gg/bugreport""
}
")));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<WebsiteConfiguration>>(It.IsAny<DataType>())).Returns(value: null);

            WebsiteConfigurationFetcher fetcher = new WebsiteConfigurationFetcher(loggerMock.Object, blobDataFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            WebsiteConfiguration? result = await fetcher.Fetch();

            // Assert
            result.Should().BeNull();
        }
    }
}
