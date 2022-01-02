using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Test.Mocks;
using TotovBuilder.AzureFunctions.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Utils
{
    /// <summary>
    /// Represents tests on the <see cref="DataFetcher"/> class.
    /// </summary>
    public class DataFetcherTest
    {
        [Theory]
        [InlineData(DataType.ItemCategories)]
        [InlineData(DataType.Items)]
        [InlineData(DataType.Presets)]
        public async Task Fetch_ShouldFetchStaticData(DataType dataType)
        {
            // Arrange
            string? itemCategoriesAzureBlobName = "item-categories";
            string? itemsAzureBlobName = "items";
            string? presetsAzureBlobName = "presets";

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemCategoriesAzureBlobNameKey)).Returns(itemCategoriesAzureBlobName);
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemsAzureBlobNameKey)).Returns(itemsAzureBlobName);
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.PresetsAzureBlobNameKey)).Returns(presetsAzureBlobName);

            Mock<IMarketDataFetcher> marketDataQuerierMock = new Mock<IMarketDataFetcher>();

            Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(itemCategoriesAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.ItemCategories)));
            blobFetcherMock.Setup(m => m.Fetch(itemsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Items)));
            blobFetcherMock.Setup(m => m.Fetch(presetsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Presets)));

            Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

            Cache cache = new Cache(loggerMock.Object);
            DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

            // Act
            await dataFetcher.Fetch(dataType);

            // Assert
            cache.Get(DataType.ItemCategories).Should().Be(TestData.ItemCategories);
            cache.Get(DataType.Items).Should().Be(TestData.Items);
            cache.Get(DataType.Presets).Should().Be(TestData.Presets);
        }

        [Fact]
        public async Task Fetch_ShouldFetchMarketData()
        {
            // Arrange
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.MarketDataCacheDurationKey)).Returns(3600);

            Mock<IMarketDataFetcher> marketDataQuerierMock = new Mock<IMarketDataFetcher>();
            marketDataQuerierMock.Setup(m => m.Fetch()).Returns(Task.FromResult(Result.Ok(TestData.MarketDataItemsOnly)));
            
            Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
            Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

            Cache cache = new Cache(loggerMock.Object);
            DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

            // Act
            await dataFetcher.Fetch(DataType.MarketData);

            // Assert
            cache.Get(DataType.MarketData).Should().Be(TestData.MarketDataItemsOnly);
        }

        [Fact]
        public async Task Fetch_ShouldFetchStaticDataFromCache()
        {
            // Arrange
            string? itemCategoriesAzureBlobName = "item-categories";
            string? itemsAzureBlobName = "items";
            string? presetsAzureBlobName = "presets";

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemCategoriesAzureBlobNameKey)).Returns(itemCategoriesAzureBlobName);
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemsAzureBlobNameKey)).Returns(itemsAzureBlobName);
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.PresetsAzureBlobNameKey)).Returns(presetsAzureBlobName);

            Mock<IMarketDataFetcher> marketDataQuerierMock = new Mock<IMarketDataFetcher>();

            Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(itemCategoriesAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.ItemCategories)));
            blobFetcherMock.Setup(m => m.Fetch(itemsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Items)));
            blobFetcherMock.Setup(m => m.Fetch(presetsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Presets)));

            Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

            Cache cache = new Cache(loggerMock.Object);
            DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

            // Act
            await dataFetcher.Fetch(DataType.ItemCategories);
            await dataFetcher.Fetch(DataType.ItemCategories);

            // Assert
            cache.Get(DataType.ItemCategories).Should().Be(TestData.ItemCategories);
            blobFetcherMock.Verify(m => m.Fetch(itemCategoriesAzureBlobName), Times.Once());
        }

        [Fact]
        public async Task Fetch_ShouldFetchMarketDataFromCache()
        {
            // Arrange
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.MarketDataCacheDurationKey)).Returns(3600);

            Mock<IMarketDataFetcher> marketDataQuerierMock = new Mock<IMarketDataFetcher>();
            marketDataQuerierMock.Setup(m => m.Fetch()).Returns(Task.FromResult(Result.Ok(TestData.MarketDataItemsOnly)));
            
            Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
            Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

            Cache cache = new Cache(loggerMock.Object);
            DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

            // Act
            await dataFetcher.Fetch(DataType.MarketData);
            await dataFetcher.Fetch(DataType.MarketData);

            // Assert
            cache.Get(DataType.MarketData).Should().Be(TestData.MarketDataItemsOnly);
            marketDataQuerierMock.Verify(m => m.Fetch(), Times.Once());
        }

        [Fact]
        public async Task Fetch_WithError_ShouldClearStaticDataFromCache()
        {
            // Arrange
            string? itemCategoriesAzureBlobName = "item-categories";
            string? itemsAzureBlobName = "items";
            string? presetsAzureBlobName = "presets";

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemCategoriesAzureBlobNameKey)).Returns(itemCategoriesAzureBlobName);
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemsAzureBlobNameKey)).Returns(itemsAzureBlobName);
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.PresetsAzureBlobNameKey)).Returns(presetsAzureBlobName);

            Mock<IMarketDataFetcher> marketDataQuerierMock = new Mock<IMarketDataFetcher>();

            Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(itemCategoriesAzureBlobName)).Returns(Task.FromResult(Result.Fail<string>(string.Empty)));
            blobFetcherMock.Setup(m => m.Fetch(itemsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Items)));
            blobFetcherMock.Setup(m => m.Fetch(presetsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Presets)));

            Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

            Cache cache = new Cache(loggerMock.Object);
            DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

            // Act
            cache.Store(DataType.ItemCategories, "TestValue");
            await dataFetcher.Fetch(DataType.ItemCategories);

            // Assert
            cache.Get(DataType.ItemCategories).Should().Be("[]");
        }

        [Fact]
        public async Task Fetch_WithError_ShouldClearMarketDataFromCache()
        {
            // Arrange
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.MarketDataCacheDurationKey)).Returns(1);

            Mock<IMarketDataFetcher> marketDataQuerierMock = new Mock<IMarketDataFetcher>();
            marketDataQuerierMock.Setup(m => m.Fetch()).Returns(Task.FromResult(Result.Fail<string>(string.Empty)));
            
            Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
            Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

            Cache cache = new Cache(loggerMock.Object);
            DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

            // Act
            cache.Store(DataType.MarketData, "TestValue");
            await Task.Delay(1500);
            await dataFetcher.Fetch(DataType.MarketData);

            // Assert
            cache.Get(DataType.MarketData).Should().Be("[]");
        }

        [Fact]
        public async Task Fetch_WithPreviousStaticDataFetch_ShouldWaitForItToFinish()
        {
            // Arrange
            string? itemCategoriesAzureBlobName = "item-categories";
            string? itemsAzureBlobName = "items";
            string? presetsAzureBlobName = "presets";

            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemCategoriesAzureBlobNameKey)).Returns(itemCategoriesAzureBlobName);
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemsAzureBlobNameKey)).Returns(itemsAzureBlobName);
            configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.PresetsAzureBlobNameKey)).Returns(presetsAzureBlobName);

            Mock<IMarketDataFetcher> marketDataQuerierMock = new Mock<IMarketDataFetcher>();

            Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(itemCategoriesAzureBlobName)).Returns(Task.Delay(1000).ContinueWith(_ => Result.Ok(TestData.ItemCategories)));
            blobFetcherMock.Setup(m => m.Fetch(itemsAzureBlobName)).Returns(Task.Delay(1000).ContinueWith(_ => Result.Ok(TestData.Items)));
            blobFetcherMock.Setup(m => m.Fetch(presetsAzureBlobName)).Returns(Task.Delay(1000).ContinueWith(_ => Result.Ok(TestData.Presets)));

            Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

            Cache cache = new Cache(loggerMock.Object);
            DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

            // Act
            Task<string> previousDataFetch = dataFetcher.Fetch(DataType.ItemCategories);
            await dataFetcher.Fetch(DataType.ItemCategories);

            // Assert
            cache.Get(DataType.ItemCategories).Should().Be(TestData.ItemCategories);
            blobFetcherMock.Verify(m => m.Fetch(itemCategoriesAzureBlobName), Times.Once());
        }

        [Fact]
        public async Task Fetch_WithPreviousMarketDataFetch_ShouldWaitForItToFinish()
        {
            // Arrange
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.MarketDataCacheDurationKey)).Returns(3600);

            Mock<IMarketDataFetcher> marketDataQuerierMock = new Mock<IMarketDataFetcher>();
            marketDataQuerierMock.Setup(m => m.Fetch()).Returns(Task.Delay(1000).ContinueWith(_ => Result.Ok(TestData.MarketDataItemsOnly)));
            
            Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
            Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

            Cache cache = new Cache(loggerMock.Object);
            DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

            // Act
            Task<string> previousDataFetch = dataFetcher.Fetch(DataType.MarketData);
            await dataFetcher.Fetch(DataType.MarketData);

            // Assert
            cache.Get(DataType.MarketData).Should().Be(TestData.MarketDataItemsOnly);
            marketDataQuerierMock.Verify(m => m.Fetch(), Times.Once());
        }
    }
}
