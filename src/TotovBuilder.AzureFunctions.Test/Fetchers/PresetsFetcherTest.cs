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
    /// Represents tests on the <see cref="PresetsFetcher"/> class.
    /// </summary>
    public class PresetsFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnPresets()
        {
            // Arrange
            Mock<ILogger<PresetsFetcher>> loggerMock = new Mock<ILogger<PresetsFetcher>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.PresetsJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            PresetsFetcher fetcher = new PresetsFetcher(loggerMock.Object, blobFetcherMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Presets);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData(@"[
  {
  }
]")]
        [InlineData(@"[
  {
    ""content"": [],
    ""ignorePrice"": false,
    ""itemId"": ""57dc2fa62459775949412633"",
    ""modSlots"": [
      {
      }
    ],
    ""quantity"": 1
  }
]")]
        [InlineData(@"[
  {
    ""content"": [],
    ""ignorePrice"": false,
    ""itemId"": ""57dc2fa62459775949412633"",
    ""modSlots"": [
      {
        ""item"": {
        },
        ""modSlotName"": ""mod_stock""
      }
    ],
    ""quantity"": 1
  }
]")]
        [InlineData(@"[
  {
    ""content"": [
      {
      }
    ],
    ""ignorePrice"": false,
    ""itemId"": ""57dc2fa62459775949412633"",
    ""modSlots"": [],
    ""quantity"": 1
  }
]")]
        public async Task Fetch_WithError_ShouldReturnNull(string presetsJson)
        {
            // Arrange
            Mock<ILogger<PresetsFetcher>> loggerMock = new Mock<ILogger<PresetsFetcher>>();
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(presetsJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<InventoryItem>>(It.IsAny<DataType>())).Returns(value: null);

            PresetsFetcher fetcher = new PresetsFetcher(loggerMock.Object, blobFetcherMock.Object, configurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeNull();
        }
    }
}
