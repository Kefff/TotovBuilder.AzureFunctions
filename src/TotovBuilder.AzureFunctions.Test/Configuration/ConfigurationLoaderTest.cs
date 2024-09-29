using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.AzureFunctions.Wrappers;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Configuration
{
    /// <summary>
    /// Represents tests on the <see cref="ConfigurationLoader"/> class.
    /// </summary>
    public class ConfigurationLoaderTest
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

                    return Result.Ok();
                })
                .Verifiable();
            azureFunctionsConfigurationFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            IConfigurationWrapper configurationWrapper = new ConfigurationWrapper();

            // Act
            ConfigurationLoader configurationLoader = new(
                new Mock<ILogger<ConfigurationLoader>>().Object,
                configurationWrapper,
                azureFunctionsConfigurationFetcherMock.Object);
            await configurationLoader.WaitForLoading();
            await configurationLoader.WaitForLoading();

            // Assert
            configurationWrapper.Values.Should().BeEquivalentTo(TestData.AzureFunctionsConfiguration);
            azureFunctionsConfigurationFetcherMock.Verify(m => m.Fetch(), Times.Once);
        }
    }
}
