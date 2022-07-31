using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Models;
using TotovBuilder.AzureFunctions.Test.Mocks;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="ChangelogFetcher"/> class.
    /// </summary>
    public class ChangelogFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnChangelog()
        {
            // Arrange
            Mock<ILogger<ChangelogFetcher>> loggerMock = new Mock<ILogger<ChangelogFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureChangelogBlobName = "changelog.json"
            });

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ChangelogJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            ChangelogFetcher fetcher = new ChangelogFetcher(loggerMock.Object, blobFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<ChangelogEntry>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
        }
    }
}
