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
    /// Represents tests on the <see cref="WebsiteConfigurationFetcher"/> class.
    /// </summary>
    public class WebsiteConfigurationFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnWebsiteConfiguration()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                RawWebsiteConfigurationBlobName = "website-configuration.json"
            });

            Mock<IAzureBlobManager> blobDataFetcherMock = new Mock<IAzureBlobManager>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.WebsiteConfigurationJson)));

            WebsiteConfigurationFetcher fetcher = new WebsiteConfigurationFetcher(
                new Mock<ILogger<WebsiteConfigurationFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<WebsiteConfiguration> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.WebsiteConfiguration);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                RawWebsiteConfigurationBlobName = "website-configuration.json"
            });

            Mock<IAzureBlobManager> blobDataFetcherMock = new Mock<IAzureBlobManager>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"{
  invalid,
  ""bugReportUrl"": ""https://discord.gg/bugreport""
}
")));

            WebsiteConfigurationFetcher fetcher = new WebsiteConfigurationFetcher(
                new Mock<ILogger<WebsiteConfigurationFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<WebsiteConfiguration> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("WebsiteConfiguration - No data fetched.");
        }
    }
}
