using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.Model;
using Xunit;
using TotovBuilder.Model.Test;

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
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();

            Cache cache = new Cache(loggerMock.Object, azureFunctionsConfigurationReaderMock.Object);
            
            // Act
            IEnumerable<ChangelogEntry>? result = cache.Get<IEnumerable<ChangelogEntry>>(DataType.Changelog);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Get_ShouldGetCachedData()
        {
            // Arrange
            Mock<ILogger<ICache>> loggerMock = new Mock<ILogger<ICache>>();
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();

            Cache cache = new Cache(loggerMock.Object, azureFunctionsConfigurationReaderMock.Object);
            cache.Store(DataType.Changelog, TestData.Changelog);

            // Act
            IEnumerable<ChangelogEntry>? result = cache.Get<IEnumerable<ChangelogEntry>>(DataType.Changelog);

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
        }

        [Fact]
        public void HasValidCache_WithoutCachedData_ShouldReturnFalse()
        {
            // Arrange
            Mock<ILogger<ICache>> loggerMock = new Mock<ILogger<ICache>>();
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();

            Cache cache = new Cache(loggerMock.Object, azureFunctionsConfigurationReaderMock.Object);
            
            // Act
            bool result = cache.HasValidCache(DataType.Changelog);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(DataType.Changelog, 3600, true)]
        [InlineData(DataType.Changelog, 1, false)]
        [InlineData(DataType.Prices, 3600, true)]
        public async void HasValidCache_ShouldIndicatedWhetherTheCacheIsValid(DataType dataType, int cacheDuration, bool expected)
        {
            // Arrange            
            Mock<ILogger<ICache>> loggerMock = new Mock<ILogger<ICache>>();
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                CacheDuration = cacheDuration
            });
            
            Cache cache = new Cache(loggerMock.Object, azureFunctionsConfigurationReaderMock.Object);
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
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();

            Cache cache = new Cache(loggerMock.Object, azureFunctionsConfigurationReaderMock.Object);
            cache.Store(DataType.Changelog, TestData.Changelog);
            IEnumerable<ChangelogEntry>? result1 = cache.Get<IEnumerable<ChangelogEntry>>(DataType.Changelog);
            
            // Act
            cache.Remove(DataType.Changelog);
            IEnumerable<ChangelogEntry>? result2 = cache.Get<IEnumerable<ChangelogEntry>>(DataType.Changelog);

            // Assert
            result1.Should().BeEquivalentTo(TestData.Changelog);
            result2.Should().BeNull();
        }

        [Fact]
        public void Store_ShouldCacheData()
        {
            // Arrange
            Mock<ILogger<ICache>> loggerMock = new Mock<ILogger<ICache>>();
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            
            // Act
            Cache cache = new Cache(loggerMock.Object, azureFunctionsConfigurationReaderMock.Object);
            cache.Store(DataType.Changelog, TestData.Changelog);

            // Act
            IEnumerable<ChangelogEntry>? result = cache.Get<IEnumerable<ChangelogEntry>>(DataType.Changelog);

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
        }
    }
}
