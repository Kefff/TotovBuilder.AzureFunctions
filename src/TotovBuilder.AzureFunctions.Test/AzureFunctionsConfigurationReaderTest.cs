using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test
{
    /// <summary>
    /// Represents tests on the <see cref="AzureFunctionsConfigurationReader"/> class.
    /// </summary>
    public class AzureFunctionsConfigurationReaderTest
    {
        [Fact]
        public async Task Load_ShouldLoadConfiguration()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationFetcher> azureFunctionsConfigurationFetcherMock = new();
            azureFunctionsConfigurationFetcherMock
                .Setup(m => m.Fetch())
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return TestData.AzureFunctionsConfiguration;
                });

            IAzureFunctionsConfigurationCache azureFunctionsConfigurationCache = new AzureFunctionsConfigurationCache();

            // Act
            AzureFunctionsConfigurationReader azureFunctionsConfigurationReader = new(
                new Mock<ILogger<AzureFunctionsConfigurationReader>>().Object,
                azureFunctionsConfigurationCache,
                azureFunctionsConfigurationFetcherMock.Object);
            _ = azureFunctionsConfigurationReader.Load();
            await azureFunctionsConfigurationReader.Load();

            // Assert
            azureFunctionsConfigurationCache.Values.Should().BeEquivalentTo(TestData.AzureFunctionsConfiguration);
            azureFunctionsConfigurationFetcherMock.Verify(m => m.Fetch(), Times.Once);
        }
    }
}
