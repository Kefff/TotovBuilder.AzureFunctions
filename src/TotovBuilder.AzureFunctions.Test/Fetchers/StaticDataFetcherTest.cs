using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="StaticDataFetcher"/> class.
    /// </summary>
    public class StaticDataFetcherTest
    {
        [Fact]
        public async Task Fetch_WithPreviousFetchingTask_ShouldWaitForItToEndAndReturnCachedData()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureChangelogBlobName = "changelog.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new();
            blobDataFetcherMock
                .Setup(m => m.Fetch(It.IsAny<string>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return Result.Ok(TestData.ChangelogJson);
                });

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>())).Returns(TestData.Changelog);

            StaticDataFetcherImplementation staticDataFetcher = new(
                new Mock<ILogger<StaticDataFetcherImplementation>>().Object,
                blobDataFetcherMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object);

            // Act
            _ = staticDataFetcher.Fetch();
            IEnumerable<ChangelogEntry>? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobDataFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<IEnumerable<ChangelogEntry>>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithValidCachedData_ShouldReturnCachedData()
        {
            // Arrange
            Mock<IBlobFetcher> blobDataFetcherMock = new();

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(true);
            cacheMock.Setup(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>())).Returns(TestData.Changelog);

            StaticDataFetcherImplementation staticDataFetcher = new(
                new Mock<ILogger<StaticDataFetcherImplementation>>().Object,
                blobDataFetcherMock.Object,
                new Mock<IAzureFunctionsConfigurationCache>().Object,
                cacheMock.Object);

            // Act
            IEnumerable<ChangelogEntry>? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobDataFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Never);
            cacheMock.Verify(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithoutValidCachedData_ShouldFetchDataAndCacheIt()
        {
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureChangelogBlobName = "changelog.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ChangelogJson)));

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            StaticDataFetcherImplementation staticDataFetcher = new(
                new Mock<ILogger<StaticDataFetcherImplementation>>().Object,
                blobDataFetcherMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object);

            // Act
            IEnumerable<ChangelogEntry>? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobDataFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>()), Times.Never);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<IEnumerable<ChangelogEntry>>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithError_ShouldReturnCachedData()
        {
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureChangelogBlobName = "changelog.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Fail<string>(string.Empty)));

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>())).Returns(TestData.Changelog);

            StaticDataFetcherImplementation staticDataFetcher = new(
                new Mock<ILogger<StaticDataFetcherImplementation>>().Object,
                blobDataFetcherMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object);

            // Act
            IEnumerable<ChangelogEntry>? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobDataFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        public class StaticDataFetcherImplementation : StaticDataFetcher<IEnumerable<ChangelogEntry>>
        {
            protected override string AzureBlobName => AzureFunctionsConfigurationCache.Values.AzureChangelogBlobName;

            protected override DataType DataType => DataType.Changelog;

            public StaticDataFetcherImplementation(
                ILogger<StaticDataFetcherImplementation> logger,
                IBlobFetcher blobFetcher,
                IAzureFunctionsConfigurationCache azureFunctionsConfigurationCache,
                ICache cache)
               : base(logger, blobFetcher, azureFunctionsConfigurationCache, cache)
            {
            }

            protected override Task<Result<IEnumerable<ChangelogEntry>>> DeserializeData(string responseContent)
            {
                return Task.FromResult(Result.Ok(TestData.Changelog.AsEnumerable()));
            }
        }
    }
}
