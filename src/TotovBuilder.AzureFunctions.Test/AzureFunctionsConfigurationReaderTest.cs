using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Test.Mocks;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test
{
    /// <summary>
    /// Represents tests on the <see cref="AzureFunctionsConfigurationReader"/> class.
    /// </summary>
    public class AzureFunctionsConfigurationReaderTest
    {
        [Fact]
        public void Constructor_ShouldLoadConfiguration()
        {
            // Arrange
            Mock<ILogger<AzureFunctionsConfigurationReader>> loggerMock = new Mock<ILogger<AzureFunctionsConfigurationReader>>();

            Mock<IAzureFunctionsConfigurationFetcher> azureFunctionsConfigurationFetcherMock = new Mock<IAzureFunctionsConfigurationFetcher>();
            azureFunctionsConfigurationFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<AzureFunctionsConfiguration?>(TestData.AzureFunctionsConfiguration));

            // Act
            AzureFunctionsConfigurationReader azureFunctionsConfigurationReader = new AzureFunctionsConfigurationReader(loggerMock.Object, azureFunctionsConfigurationFetcherMock.Object);

            // Assert
            azureFunctionsConfigurationReader.Values.Should().BeEquivalentTo(TestData.AzureFunctionsConfiguration);
        }

        [Fact]
        public void Constructor_WithoutConfigurationData_ShouldThrow()
        {
            // Arrange
            Mock<ILogger<AzureFunctionsConfigurationReader>> loggerMock = new Mock<ILogger<AzureFunctionsConfigurationReader>>();

            Mock<IAzureFunctionsConfigurationFetcher> azureFunctionsConfigurationFetcherMock = new Mock<IAzureFunctionsConfigurationFetcher>();
            azureFunctionsConfigurationFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<AzureFunctionsConfiguration?>(null));

            // Act
            AzureFunctionsConfigurationReader azureFunctionsConfigurationReader = new AzureFunctionsConfigurationReader(loggerMock.Object, azureFunctionsConfigurationFetcherMock.Object);
            Func<Task> act = async () => { await azureFunctionsConfigurationReader.WaitUntilReady(); };

            // Assert
            act.Should().ThrowAsync<Exception>("Invalid configuration");
        }
    }
}
