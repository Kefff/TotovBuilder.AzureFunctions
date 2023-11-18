using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Cache;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Cache;
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

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.AzureFunctionsConfigurationJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            AzureFunctionsConfigurationFetcher fetcher = new AzureFunctionsConfigurationFetcher(
                new Mock<ILogger<AzureFunctionsConfigurationFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object,
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
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureFunctionsConfigurationBlobName = "azure-functions-configuration.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok("invalid")));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<AzureFunctionsConfiguration>(It.IsAny<DataType>())).Returns(value: null);

            AzureFunctionsConfigurationFetcher fetcher = new AzureFunctionsConfigurationFetcher(
                new Mock<ILogger<AzureFunctionsConfigurationFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object,
                cacheMock.Object);

            // Act
            Func<Task> act = () => fetcher.Fetch();

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }
    }
}
