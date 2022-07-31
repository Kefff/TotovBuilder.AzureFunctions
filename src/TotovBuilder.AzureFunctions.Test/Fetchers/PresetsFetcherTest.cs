using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Models.Builds;
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

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzurePresetsBlobName = "presets.json"
            });

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.PresetsJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            PresetsFetcher fetcher = new PresetsFetcher(loggerMock.Object, blobFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<InventoryItem>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Presets);
        }
    }
}
