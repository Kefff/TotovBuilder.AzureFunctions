using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model;
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
            Mock<ILogger<AzureFunctionsConfigurationFetcher>> loggerMock = new Mock<ILogger<AzureFunctionsConfigurationFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new Model.AzureFunctionsConfiguration()
            {
                AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json"
            });

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.AzureFunctionsConfigurationJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            AzureFunctionsConfigurationFetcher fetcher = new AzureFunctionsConfigurationFetcher(loggerMock.Object, blobFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            AzureFunctionsConfiguration? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.AzureFunctionsConfiguration);
        }

        [Fact]
        public async void Fetch_WithInvalidData_ShouldReturnNull()
        {
            // Arrange
            Mock<ILogger<AzureFunctionsConfigurationFetcher>> loggerMock = new Mock<ILogger<AzureFunctionsConfigurationFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json"
            });

            Mock<IBlobFetcher> blobFetcherMock = new Mock<IBlobFetcher>();
            blobFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok("invalid")));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<AzureFunctionsConfiguration>(It.IsAny<DataType>())).Returns(value: null);

            AzureFunctionsConfigurationFetcher fetcher = new AzureFunctionsConfigurationFetcher(loggerMock.Object, blobFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            AzureFunctionsConfiguration? result = await fetcher.Fetch();

            // Assert
            result.Should().BeNull();
        }
    }
}
