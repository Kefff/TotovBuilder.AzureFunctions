using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="AzureFunctionsConfigurationFetcher"/> class.
    /// </summary>
    public class AzureFunctionsConfigurationFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnAzureFunctionsConfiguration()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.AzureFunctionsConfigurationJson)));

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            AzureFunctionsConfigurationFetcher fetcher = new(
                new Mock<ILogger<AzureFunctionsConfigurationFetcher>>().Object,
                blobDataFetcherMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object);

            // Act
            AzureFunctionsConfiguration? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.AzureFunctionsConfiguration);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnNull()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok("invalid")));

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<AzureFunctionsConfiguration>(It.IsAny<DataType>())).Returns(value: null);

            AzureFunctionsConfigurationFetcher fetcher = new(
                new Mock<ILogger<AzureFunctionsConfigurationFetcher>>().Object,
                blobDataFetcherMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object);

            // Act
            AzureFunctionsConfiguration? result = await fetcher.Fetch();

            // Assert
            result.Should().BeNull();
        }
    }
}
