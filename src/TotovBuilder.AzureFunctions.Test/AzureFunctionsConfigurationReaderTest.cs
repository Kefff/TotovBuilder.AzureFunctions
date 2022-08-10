using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.Model.Configuration;
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
            Mock<ILogger<AzureFunctionsConfigurationReader>> loggerMock = new Mock<ILogger<AzureFunctionsConfigurationReader>>();

            Mock<IAzureFunctionsConfigurationFetcher> azureFunctionsConfigurationFetcherMock = new Mock<IAzureFunctionsConfigurationFetcher>();
            azureFunctionsConfigurationFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<AzureFunctionsConfiguration?>(TestData.AzureFunctionsConfiguration));

            AzureFunctionsConfigurationWrapper azureFunctionsConfigurationWrapper = new AzureFunctionsConfigurationWrapper();

            // Act
            AzureFunctionsConfigurationReader azureFunctionsConfigurationReader = new AzureFunctionsConfigurationReader(loggerMock.Object, azureFunctionsConfigurationWrapper, azureFunctionsConfigurationFetcherMock.Object);
            await azureFunctionsConfigurationReader.Load();
            await azureFunctionsConfigurationReader.Load();

            // Assert
            azureFunctionsConfigurationWrapper.Values.Should().BeEquivalentTo(TestData.AzureFunctionsConfiguration);
            azureFunctionsConfigurationFetcherMock.Verify(m => m.Fetch(), Times.Once);
        }

        [Fact]
        public void Load_WithoutConfigurationData_ShouldThrow()
        {
            // Arrange
            Mock<ILogger<AzureFunctionsConfigurationReader>> loggerMock = new Mock<ILogger<AzureFunctionsConfigurationReader>>();

            Mock<IAzureFunctionsConfigurationFetcher> azureFunctionsConfigurationFetcherMock = new Mock<IAzureFunctionsConfigurationFetcher>();
            azureFunctionsConfigurationFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<AzureFunctionsConfiguration?>(null));

            AzureFunctionsConfigurationWrapper azureFunctionsConfigurationWrapper = new AzureFunctionsConfigurationWrapper();

            AzureFunctionsConfigurationReader azureFunctionsConfigurationReader = new AzureFunctionsConfigurationReader(
                loggerMock.Object,
                azureFunctionsConfigurationWrapper,
                azureFunctionsConfigurationFetcherMock.Object);

            // Act
            Func<Task> act = async () => { await azureFunctionsConfigurationReader.Load(); };

            // Assert
            act.Should().ThrowAsync<Exception>("Invalid configuration");
        }
    }
}
