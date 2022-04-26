using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Test.Mocks;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Utils
{
    /// <summary>
    /// Represents tests on the <see cref="DataFetcher"/> class.
    /// </summary>
    public class DataFetcherTest
    {
        //[Theory]
        //[InlineData(DataType.ItemCategories)]
        //[InlineData(DataType.Items)]
        //[InlineData(DataType.Presets)]
        //public async Task Fetch_ShouldFetchStaticData(DataType dataType)
        //{
        //    // Arrange
        //    string? itemCategoriesAzureBlobName = "item-categories";
        //    string? itemsAzureBlobName = "items";
        //    string? presetsAzureBlobName = "presets";

        //    Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemCategoriesAzureBlobNameKey)).Returns(itemCategoriesAzureBlobName);
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemsAzureBlobNameKey)).Returns(itemsAzureBlobName);
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.PresetsAzureBlobNameKey)).Returns(presetsAzureBlobName);

        //    Mock<IItemsMetadataFetcher> marketDataQuerierMock = new Mock<IItemsMetadataFetcher>();

        //    Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
        //    blobFetcherMock.Setup(m => m.Fetch(itemCategoriesAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.ItemCategories)));
        //    blobFetcherMock.Setup(m => m.Fetch(itemsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Items)));
        //    blobFetcherMock.Setup(m => m.Fetch(presetsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Presets)));

        //    Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

        //    Cache cache = new Cache(loggerMock.Object);
        //    DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

        //    // Act
        //    await dataFetcher.Fetch(dataType);

        //    // Assert
        //    cache.Get(DataType.ItemCategories).Should().Be(TestData.ItemCategories);
        //    cache.Get(DataType.Items).Should().Be(TestData.Items);
        //    cache.Get(DataType.Presets).Should().Be(TestData.Presets);
        //}

        //[Fact]
        //public async Task Fetch_ShouldFetchMarketData()
        //{
        //    // Arrange
        //    Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
        //    configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.MarketDataCacheDurationKey)).Returns(3600);

        //    Mock<IItemsMetadataFetcher> marketDataQuerierMock = new Mock<IItemsMetadataFetcher>();
        //    marketDataQuerierMock.Setup(m => m.Fetch()).Returns(Task.FromResult(Result.Ok(TestData.MarketDataItemsOnly)));
            
        //    Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
        //    Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

        //    Cache cache = new Cache(loggerMock.Object);
        //    DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

        //    // Act
        //    await dataFetcher.Fetch(DataType.ItemsMetadata);

        //    // Assert
        //    cache.Get(DataType.ItemsMetadata).Should().Be(TestData.MarketDataItemsOnly);
        //}

        //[Fact]
        //public async Task Fetch_ShouldFetchStaticDataFromCache()
        //{
        //    // Arrange
        //    string? itemCategoriesAzureBlobName = "item-categories";
        //    string? itemsAzureBlobName = "items";
        //    string? presetsAzureBlobName = "presets";

        //    Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemCategoriesAzureBlobNameKey)).Returns(itemCategoriesAzureBlobName);
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemsAzureBlobNameKey)).Returns(itemsAzureBlobName);
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.PresetsAzureBlobNameKey)).Returns(presetsAzureBlobName);

        //    Mock<IItemsMetadataFetcher> marketDataQuerierMock = new Mock<IItemsMetadataFetcher>();

        //    Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
        //    blobFetcherMock.Setup(m => m.Fetch(itemCategoriesAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.ItemCategories)));
        //    blobFetcherMock.Setup(m => m.Fetch(itemsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Items)));
        //    blobFetcherMock.Setup(m => m.Fetch(presetsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Presets)));

        //    Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

        //    Cache cache = new Cache(loggerMock.Object);
        //    DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

        //    // Act
        //    await dataFetcher.Fetch(DataType.ItemCategories);
        //    await dataFetcher.Fetch(DataType.ItemCategories);

        //    // Assert
        //    cache.Get(DataType.ItemCategories).Should().Be(TestData.ItemCategories);
        //    blobFetcherMock.Verify(m => m.Fetch(itemCategoriesAzureBlobName), Times.Once());
        //}

        //[Fact]
        //public async Task Fetch_ShouldFetchMarketDataFromCache()
        //{
        //    // Arrange
        //    Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
        //    configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.MarketDataCacheDurationKey)).Returns(3600);

        //    Mock<IItemsMetadataFetcher> marketDataQuerierMock = new Mock<IItemsMetadataFetcher>();
        //    marketDataQuerierMock.Setup(m => m.Fetch()).Returns(Task.FromResult(Result.Ok(TestData.MarketDataItemsOnly)));
            
        //    Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
        //    Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

        //    Cache cache = new Cache(loggerMock.Object);
        //    DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

        //    // Act
        //    await dataFetcher.Fetch(DataType.ItemsMetadata);
        //    await dataFetcher.Fetch(DataType.ItemsMetadata);

        //    // Assert
        //    cache.Get(DataType.ItemsMetadata).Should().Be(TestData.MarketDataItemsOnly);
        //    marketDataQuerierMock.Verify(m => m.Fetch(), Times.Once());
        //}

        //[Fact]
        //public async Task Fetch_WithError_ShouldReturnOldStaticDataFromCache()
        //{
        //    // Arrange
        //    string? itemCategoriesAzureBlobName = "item-categories";
        //    string? itemsAzureBlobName = "items";
        //    string? presetsAzureBlobName = "presets";

        //    Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemCategoriesAzureBlobNameKey)).Returns(itemCategoriesAzureBlobName);
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemsAzureBlobNameKey)).Returns(itemsAzureBlobName);
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.PresetsAzureBlobNameKey)).Returns(presetsAzureBlobName);

        //    Mock<IItemsMetadataFetcher> marketDataQuerierMock = new Mock<IItemsMetadataFetcher>();

        //    Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
        //    blobFetcherMock.Setup(m => m.Fetch(itemCategoriesAzureBlobName)).Returns(Task.FromResult(Result.Fail<string>(string.Empty)));
        //    blobFetcherMock.Setup(m => m.Fetch(itemsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Items)));
        //    blobFetcherMock.Setup(m => m.Fetch(presetsAzureBlobName)).Returns(Task.FromResult(Result.Ok(TestData.Presets)));

        //    Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

        //    Cache cache = new Cache(loggerMock.Object);
        //    DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

        //    // Act
        //    cache.Store(DataType.ItemCategories, "OldValue");
        //    await dataFetcher.Fetch(DataType.ItemCategories);

        //    // Assert
        //    cache.Get(DataType.ItemCategories).Should().Be("OldValue");
        //}

        //[Fact]
        //public async Task Fetch_WithError_ShouldReturnOldMarketDataFromCache()
        //{
        //    // Arrange
        //    Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
        //    configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.MarketDataCacheDurationKey)).Returns(1);

        //    Mock<IItemsMetadataFetcher> marketDataQuerierMock = new Mock<IItemsMetadataFetcher>();
        //    marketDataQuerierMock.Setup(m => m.Fetch()).Returns(Task.FromResult(Result.Fail<string>(string.Empty)));
            
        //    Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
        //    Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

        //    Cache cache = new Cache(loggerMock.Object);
        //    DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

        //    // Act
        //    cache.Store(DataType.ItemsMetadata, "OldValue");
        //    await Task.Delay(1500);
        //    await dataFetcher.Fetch(DataType.ItemsMetadata);

        //    // Assert
        //    cache.Get(DataType.ItemsMetadata).Should().Be("OldValue");
        //}

        //[Fact]
        //public async Task Fetch_WithPreviousStaticDataFetch_ShouldWaitForItToFinish()
        //{
        //    // Arrange
        //    string? itemCategoriesAzureBlobName = "item-categories";
        //    string? itemsAzureBlobName = "items";
        //    string? presetsAzureBlobName = "presets";

        //    Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemCategoriesAzureBlobNameKey)).Returns(itemCategoriesAzureBlobName);
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.ItemsAzureBlobNameKey)).Returns(itemsAzureBlobName);
        //    configurationReaderMock.Setup(m => m.ReadString(ConfigurationReader.PresetsAzureBlobNameKey)).Returns(presetsAzureBlobName);

        //    Mock<IItemsMetadataFetcher> marketDataQuerierMock = new Mock<IItemsMetadataFetcher>();

        //    Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
        //    blobFetcherMock.Setup(m => m.Fetch(itemCategoriesAzureBlobName)).Returns(Task.Delay(1000).ContinueWith(_ => Result.Ok(TestData.ItemCategories)));
        //    blobFetcherMock.Setup(m => m.Fetch(itemsAzureBlobName)).Returns(Task.Delay(1000).ContinueWith(_ => Result.Ok(TestData.Items)));
        //    blobFetcherMock.Setup(m => m.Fetch(presetsAzureBlobName)).Returns(Task.Delay(1000).ContinueWith(_ => Result.Ok(TestData.Presets)));

        //    Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

        //    Cache cache = new Cache(loggerMock.Object);
        //    DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

        //    // Act
        //    Task<string> previousDataFetch = dataFetcher.Fetch(DataType.ItemCategories);
        //    await dataFetcher.Fetch(DataType.ItemCategories);

        //    // Assert
        //    cache.Get(DataType.ItemCategories).Should().Be(TestData.ItemCategories);
        //    blobFetcherMock.Verify(m => m.Fetch(itemCategoriesAzureBlobName), Times.Once());
        //}

        //[Fact]
        //public async Task Fetch_WithPreviousMarketDataFetch_ShouldWaitForItToFinish()
        //{
        //    // Arrange
        //    Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
        //    configurationReaderMock.Setup(m => m.ReadInt(ConfigurationReader.MarketDataCacheDurationKey)).Returns(3600);

        //    Mock<IItemsMetadataFetcher> marketDataQuerierMock = new Mock<IItemsMetadataFetcher>();
        //    marketDataQuerierMock.Setup(m => m.Fetch()).Returns(Task.Delay(1000).ContinueWith(_ => Result.Ok(TestData.MarketDataItemsOnly)));
            
        //    Mock<IBlobDataFetcher> blobFetcherMock = new Mock<IBlobDataFetcher>();
        //    Mock<ILogger<DataFetcher>> loggerMock = new Mock<ILogger<DataFetcher>>();

        //    Cache cache = new Cache(loggerMock.Object);
        //    DataFetcher dataFetcher = new DataFetcher(loggerMock.Object, marketDataQuerierMock.Object, cache, configurationReaderMock.Object, blobFetcherMock.Object);

        //    // Act
        //    Task<string> previousDataFetch = dataFetcher.Fetch(DataType.ItemsMetadata);
        //    await dataFetcher.Fetch(DataType.ItemsMetadata);

        //    // Assert
        //    cache.Get(DataType.ItemsMetadata).Should().Be(TestData.MarketDataItemsOnly);
        //    marketDataQuerierMock.Verify(m => m.Fetch(), Times.Once());
        //}
    }
}
