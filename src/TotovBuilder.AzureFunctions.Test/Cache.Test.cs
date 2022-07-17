using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Models;
using TotovBuilder.AzureFunctions.Test.Mocks;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test
{
    /// <summary>
    /// Represents tests on the <see cref="Cache"/> class.
    /// </summary>
    public class CacheTest
    {
        [Fact]
        public void Get_WithoutCachedData_ShouldReturnNothing()
        {
            // Arrange
            Mock<ILogger<ICache>> loggerMock = new Mock<ILogger<ICache>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();

            Cache cache = new Cache(loggerMock.Object, configurationReaderMock.Object);
            
            // Act
            ItemCategory[]? result = cache.Get<ItemCategory[]>(DataType.ItemCategories);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Get_ShouldGetCachedData()
        {
            // Arrange
            Mock<ILogger<ICache>> loggerMock = new Mock<ILogger<ICache>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();

            Cache cache = new Cache(loggerMock.Object, configurationReaderMock.Object);
            cache.Store(DataType.ItemCategories, TestData.ItemCategories);

            // Act
            ItemCategory[]? result = cache.Get<ItemCategory[]>(DataType.ItemCategories);

            // Assert
            result.Should().BeEquivalentTo(TestData.ItemCategories);
        }

        [Fact]
        public void HasValidCache_WithoutCachedData_ShouldReturnFalse()
        {
            // Arrange
            Mock<ILogger<ICache>> loggerMock = new Mock<ILogger<ICache>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();

            Cache cache = new Cache(loggerMock.Object, configurationReaderMock.Object);
            
            // Act
            bool result = cache.HasValidCache(DataType.ItemCategories);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(DataType.ItemCategories, 3600, true)]
        [InlineData(DataType.ItemCategories, 1, false)]
        [InlineData(DataType.Prices, 3600, true)]
        public async void HasValidCache_ShouldIndicatedWhetherTheCacheIsValid(DataType dataType, int cacheDuration, bool expected)
        {
            // Arrange            
            Mock<ILogger<ICache>> loggerMock = new Mock<ILogger<ICache>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            configurationReaderMock.Setup(m => m.ReadInt(It.IsAny<string>())).Returns(cacheDuration);
            
            Cache cache = new Cache(loggerMock.Object, configurationReaderMock.Object);
            cache.Store(dataType, "Test");

            // Act
            await Task.Delay(2000);
            bool result = cache.HasValidCache(dataType);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Remove_ShouldRemoveCachedData()
        {
            // Arrange
            Mock<ILogger<ICache>> loggerMock = new Mock<ILogger<ICache>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();

            Cache cache = new Cache(loggerMock.Object, configurationReaderMock.Object);
            cache.Store(DataType.ItemCategories, TestData.ItemCategories);
            ItemCategory[]? result1 = cache.Get<ItemCategory[]>(DataType.ItemCategories);
            
            // Act
            cache.Remove(DataType.ItemCategories);
            ItemCategory[]? result2 = cache.Get<ItemCategory[]>(DataType.ItemCategories);

            // Assert
            result1.Should().BeEquivalentTo(TestData.ItemCategories);
            result2.Should().BeNull();
        }

        [Fact]
        public void Store_ShouldCacheData()
        {
            // Arrange
            Mock<ILogger<ICache>> loggerMock = new Mock<ILogger<ICache>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            
            // Act
            Cache cache = new Cache(loggerMock.Object, configurationReaderMock.Object);
            cache.Store(DataType.ItemCategories, TestData.ItemCategories);

            // Act
            ItemCategory[]? result = cache.Get<ItemCategory[]>(DataType.ItemCategories);

            // Assert
            result.Should().BeEquivalentTo(TestData.ItemCategories);
        }
    }
}
