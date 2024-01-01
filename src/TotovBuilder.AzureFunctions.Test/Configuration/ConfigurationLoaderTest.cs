using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Configuration;
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
            Mock<IAzureFunctionsConfigurationFetcher> azureFunctionsConfigurationFetcherMock = new Mock<IAzureFunctionsConfigurationFetcher>();
            azureFunctionsConfigurationFetcherMock
                .Setup(m => m.Fetch())
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return TestData.AzureFunctionsConfiguration;
                });

            IConfigurationWrapper configurationWrapper = new ConfigurationWrapper();

            // Act
            ConfigurationLoader configurationLoader = new ConfigurationLoader(
                new Mock<ILogger<ConfigurationLoader>>().Object,
                configurationWrapper,
                azureFunctionsConfigurationFetcherMock.Object);
            _ = configurationLoader.Load();
            await configurationLoader.Load();

            // Assert
            configurationWrapper.Values.Should().BeEquivalentTo(TestData.AzureFunctionsConfiguration);
            azureFunctionsConfigurationFetcherMock.Verify(m => m.Fetch(), Times.Once);
        }
    }
}
