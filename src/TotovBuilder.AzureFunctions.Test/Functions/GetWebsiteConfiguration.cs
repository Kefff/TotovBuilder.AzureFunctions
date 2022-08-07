using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.Model;
using Xunit;
using TotovBuilder.Model.Test;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetWebsiteConfiguration"/> class.
    /// </summary>
    public class GetWebsiteConfigurationTest
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new Mock<IWebsiteConfigurationFetcher>();
            websiteConfigurationFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<WebsiteConfiguration?>(TestData.WebsiteConfiguration));

            GetWebsiteConfiguration function = new GetWebsiteConfiguration(websiteConfigurationFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(TestData.WebsiteConfiguration);
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new Mock<IWebsiteConfigurationFetcher>();
            websiteConfigurationFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<WebsiteConfiguration?>(null));

            GetWebsiteConfiguration function = new GetWebsiteConfiguration(websiteConfigurationFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(new WebsiteConfiguration());
        }
    }
}
