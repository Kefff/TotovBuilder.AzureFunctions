using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Cache;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Cache;
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
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureChangelogBlobName = "changelog.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock
                .Setup(m => m.Fetch(It.IsAny<string>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return Result.Ok(TestData.ChangelogJson);
                });

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>())).Returns(TestData.Changelog);

            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(
                new Mock<ILogger<StaticDataFetcherImplementation>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object,
                cacheMock.Object);

            // Act
            _ = staticDataFetcher.Fetch();
            IEnumerable<ChangelogEntry>? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobDataFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<IEnumerable<ChangelogEntry>>(), true), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithValidCachedData_ShouldReturnCachedData()
        {
            // Arrange
            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(true);
            cacheMock.Setup(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>())).Returns(TestData.Changelog);

            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(
                new Mock<ILogger<StaticDataFetcherImplementation>>().Object,
                blobDataFetcherMock.Object,
                new Mock<IConfigurationWrapper>().Object,
                cacheMock.Object);

            // Act
            IEnumerable<ChangelogEntry>? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobDataFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Never);
            cacheMock.Verify(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>(), true), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithoutValidCachedData_ShouldFetchDataAndCacheIt()
        {
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureChangelogBlobName = "changelog.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ChangelogJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(
                new Mock<ILogger<StaticDataFetcherImplementation>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object,
                cacheMock.Object);

            // Act
            IEnumerable<ChangelogEntry>? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobDataFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>()), Times.Never);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<IEnumerable<ChangelogEntry>>(), true), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithError_ShouldReturnCachedData()
        {
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureChangelogBlobName = "changelog.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Fail<string>(string.Empty)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>())).Returns(TestData.Changelog);

            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(
                new Mock<ILogger<StaticDataFetcherImplementation>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object,
                cacheMock.Object);

            // Act
            IEnumerable<ChangelogEntry>? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobDataFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>(), true), Times.Never);
        }

        public class StaticDataFetcherImplementation : StaticDataFetcher<IEnumerable<ChangelogEntry>>
        {
            protected override string AzureBlobName => ConfigurationWrapper.Values.AzureChangelogBlobName;

            protected override DataType DataType => DataType.Changelog;

            public StaticDataFetcherImplementation(
                ILogger<StaticDataFetcherImplementation> logger,
                IBlobFetcher blobFetcher,
                IConfigurationWrapper configurationWrapper,
                ICache cache)
               : base(logger, blobFetcher, configurationWrapper, cache)
            {
            }

            protected override Task<Result<IEnumerable<ChangelogEntry>>> DeserializeData(string responseContent)
            {
                return Task.FromResult(Result.Ok(TestData.Changelog.AsEnumerable()));
            }
        }
    }
}
