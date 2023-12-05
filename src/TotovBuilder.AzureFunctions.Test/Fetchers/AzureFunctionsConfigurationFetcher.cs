using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Utils;
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
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json"
            });

            Mock<IAzureBlobManager> blobDataFetcherMock = new Mock<IAzureBlobManager>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.AzureFunctionsConfigurationJson)));

            AzureFunctionsConfigurationFetcher fetcher = new AzureFunctionsConfigurationFetcher(
                new Mock<ILogger<AzureFunctionsConfigurationFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<AzureFunctionsConfiguration> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.AzureFunctionsConfiguration);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json"
            });

            Mock<IAzureBlobManager> blobDataFetcherMock = new Mock<IAzureBlobManager>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok("invalid")));

            AzureFunctionsConfigurationFetcher fetcher = new AzureFunctionsConfigurationFetcher(
                new Mock<ILogger<AzureFunctionsConfigurationFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<AzureFunctionsConfiguration> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("AzureFunctionsConfiguration - No data fetched.");
        }
    }
}
