﻿using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
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
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            
            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock
                .Setup(m => m.Fetch(It.IsAny<string>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return Result.Ok(TestData.ItemCategories);
                });
            
            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get(It.IsAny<DataType>())).Returns(TestData.ItemCategories);

            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(loggerMock.Object, blobFetcherMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            _ = staticDataFetcher.Fetch();
            string result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().Be(TestData.ItemCategories);
            blobFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithValidCachedData_ShouldReturnCachedData()
        {
            // Arrange
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(true);
            cacheMock.Setup(m => m.Get(It.IsAny<DataType>())).Returns(TestData.ItemCategories);

            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(loggerMock.Object, blobFetcherMock.Object, configurationReaderMock.Object, cacheMock.Object);
            
            // Act
            string result = await staticDataFetcher.Fetch();
            
            // Assert
            result.Should().Be(TestData.ItemCategories);
            blobFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Never);
            cacheMock.Verify(m => m.Get(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Fetch_WithoutValidCachedData_ShouldFetchDataAndCacheIt()
        {
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ItemCategories)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            
            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(loggerMock.Object, blobFetcherMock.Object, configurationReaderMock.Object, cacheMock.Object);
            
            // Act
            string result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().Be(TestData.ItemCategories);
            blobFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get(It.IsAny<DataType>()), Times.Never);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Once);
        }
        
        [Fact]
        public async Task Fetch_WithError_ShouldReturnCachedData()
        {
            Mock<ILogger<ApiFetcherImplementation>> loggerMock = new Mock<ILogger<ApiFetcherImplementation>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Fail<string>(string.Empty)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.Get(It.IsAny<DataType>())).Returns(TestData.ItemCategories);
            
            StaticDataFetcherImplementation staticDataFetcher = new StaticDataFetcherImplementation(loggerMock.Object, blobFetcherMock.Object, configurationReaderMock.Object, cacheMock.Object);
            
            // Act
            string result = await staticDataFetcher.Fetch();

            // Assert
            result.Should().Be(TestData.ItemCategories);
            blobFetcherMock.Verify(m => m.Fetch(It.IsAny<string>()), Times.Once);
            cacheMock.Verify(m => m.Get(It.IsAny<DataType>()), Times.Once);
            cacheMock.Verify(m => m.Store(It.IsAny<DataType>(), It.IsAny<string>()), Times.Never);
        }
    }
    
    public class StaticDataFetcherImplementation : StaticDataFetcher
    {
        protected override string AzureBlobName => TotovBuilder.AzureFunctions.ConfigurationReader.ItemCategoriesAzureBlobNameKey;

        protected override DataType DataType => DataType.ItemCategories;

        public StaticDataFetcherImplementation(ILogger logger, IBlobFetcher blobFetcher, IConfigurationReader configurationReader, ICache cache) 
           : base(logger, blobFetcher, configurationReader, cache)
        {
        }

        protected override Result<string> GetData(string responseContent)
        {
            return Result.Ok(responseContent);
        }
    }
}