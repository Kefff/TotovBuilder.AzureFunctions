using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Test.Mocks;
using TotovBuilder.AzureFunctions.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Utils
{
    /// <summary>
    /// Represents tests on the <see cref="Cache"/> class.
    /// </summary>
    public class CacheTest
    {
        [Fact]
        public void Get_ShouldGetData()
        {
            // Arrange
            Mock<ILogger> loggerMock = new Mock<ILogger>();
            Cache cache = new Cache(loggerMock.Object);

            // Act
            string itemCategories = cache.Get(DataType.ItemCategories);
            string items = cache.Get(DataType.Items);
            string marketData = cache.Get(DataType.MarketData);
            string presets = cache.Get(DataType.ItemCategories);

            // Assert
            itemCategories.Should().Be("[]");
            items.Should().Be("[]");
            marketData.Should().Be("[]");
            presets.Should().Be("[]");
        }

        [Fact]
        public void Remove_ShouldRemoveData()
        {
            // Arrange
            Mock<ILogger> loggerMock = new Mock<ILogger>();
            Cache cache = new Cache(loggerMock.Object);

            // Act
            cache.Store(DataType.ItemCategories, TestData.ItemCategories);
            cache.Store(DataType.Items, TestData.Items);
            cache.Store(DataType.MarketData, TestData.MarketData);
            cache.Store(DataType.Presets, TestData.Presets);
            cache.Remove(DataType.ItemCategories);
            cache.Remove(DataType.Items);
            cache.Remove(DataType.MarketData);
            cache.Remove(DataType.Presets);

            // Assert
            cache.Get(DataType.ItemCategories).Should().Be("[]");
            cache.Get(DataType.Items).Should().Be("[]");
            cache.Get(DataType.MarketData).Should().Be("[]");
            cache.Get(DataType.Presets).Should().Be("[]");
        }

        [Fact]
        public void Store_ShouldStoreData()
        {
            // Arrange
            Mock<ILogger> loggerMock = new Mock<ILogger>();
            Cache cache = new Cache(loggerMock.Object);

            // Act
            cache.Store(DataType.ItemCategories, TestData.ItemCategories);
            cache.Store(DataType.Items, TestData.Items);
            cache.Store(DataType.MarketData, TestData.MarketData);
            cache.Store(DataType.Presets, TestData.Presets);
            
            string itemCategories = cache.Get(DataType.ItemCategories);
            string items = cache.Get(DataType.Items);
            string marketData = cache.Get(DataType.MarketData);
            string presets = cache.Get(DataType.Presets);

            // Assert
            itemCategories.Should().Be(TestData.ItemCategories);
            items.Should().Be(TestData.Items);
            marketData.Should().Be(TestData.MarketData);
            presets.Should().Be(TestData.Presets);
        }
    }
}
