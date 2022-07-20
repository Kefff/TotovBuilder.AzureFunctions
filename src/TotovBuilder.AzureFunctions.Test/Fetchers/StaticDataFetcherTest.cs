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
    /// Represents tests on the <see cref="StaticDataFetcher"/> class.
    /// </summary>
    public class StaticDataFetcherTest
    {
        [Fact]
        public async Task Fetch_WithPreviousFetchingTask_ShouldWaitForItToEndAndReturnCachedData()
        {
            // Arrange
            Mock<ILogger<StaticDataFetcherImplementation>> loggerMock = new Mock<ILogger<StaticDataFetcherImplementation>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            
            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock
                .Setup(m => m.Fetch(It.IsAny<string>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return Result.Ok(TestData.ChangelogJson);
                });
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get<ChangelogEntry[]>(It.IsAny<DataType>())).Returns(TestData.Changelog);

            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(loggerMock.Object, blobFetcherMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            _ = staticDataFetcher.Fetch();
            ChangelogEntry[]? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get<ChangelogEntry[]>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<ChangelogEntry[]>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithValidCachedData_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<StaticDataFetcherImplementation>> loggerMock = new Mock<ILogger<StaticDataFetcherImplementation>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(true);
            cacheMock.Setup(m => m.Get<ChangelogEntry[]>(It.IsAny<DataType>())).Returns(TestData.Changelog);

            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(loggerMock.Object, blobFetcherMock.Object, configurationReaderMock.Object, cacheMock.Object);
            
            // Act
            ChangelogEntry[]? result = await staticDataFetcher.Fetch();
            
            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Never);
            cacheMock.Verify(m => m.Get<ChangelogEntry[]>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithoutValidCachedData_ShouldFetchDataAndCacheIt()
        {
            Mock<ILogger<StaticDataFetcherImplementation>> loggerMock = new Mock<ILogger<StaticDataFetcherImplementation>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ChangelogJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            
            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(loggerMock.Object, blobFetcherMock.Object, configurationReaderMock.Object, cacheMock.Object);
            
            // Act
            ChangelogEntry[]? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get<ChangelogEntry[]>(It.IsAny<DataType>()), Times.Never);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<ChangelogEntry[]>()), Times.Once);
        }
        
        [Fact]
        public async Task Fetch_WithError_ShouldReturnCachedData()
        {
            Mock<ILogger<StaticDataFetcherImplementation>> loggerMock = new Mock<ILogger<StaticDataFetcherImplementation>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Fail<string>(string.Empty)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get<ChangelogEntry[]>(It.IsAny<DataType>())).Returns(TestData.Changelog);
            
            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(loggerMock.Object, blobFetcherMock.Object, configurationReaderMock.Object, cacheMock.Object);
            
            // Act
            ChangelogEntry[]? result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
            blobFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get<ChangelogEntry[]>(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }
    
        public class StaticDataFetcherImplementation : StaticDataFetcher<ChangelogEntry[]>
        {
            protected override string AzureBlobName => TotovBuilder.AzureFunctions.ConfigurationReader.AzureChangelogBlobNameKey;

            protected override DataType DataType => DataType.Changelog;

            public StaticDataFetcherImplementation(ILogger logger, IBlobFetcher blobFetcher, IConfigurationReader configurationReader, ICache cache) 
               : base(logger, blobFetcher, configurationReader, cache)
            {
            }

            protected override Result<ChangelogEntry[]> GetData(string responseContent)
            {
                return Result.Ok(TestData.Changelog);
            }
        }
    }
}
