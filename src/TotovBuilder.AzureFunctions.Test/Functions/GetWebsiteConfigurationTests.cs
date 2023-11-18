using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Test.Net.Mocks;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetWebsiteConfiguration"/> class.
    /// </summary>
    public class GetWebsiteConfigurationTests
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            Mock<IConfigurationLoader> configurationLoaderMock = new Mock<IConfigurationLoader>();

            Mock<IHttpResponseDataFactory> httpResponseDataFactoryMock = new Mock<IHttpResponseDataFactory>();
            httpResponseDataFactoryMock
                .Setup(m => m.CreateResponse(It.IsAny<HttpRequestData>(), It.IsAny<object>()))
                .Returns(Task.FromResult((HttpResponseData)new Mock<HttpResponseDataImplementation>().Object));

            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new Mock<IWebsiteConfigurationFetcher>();
            websiteConfigurationFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(TestData.WebsiteConfiguration));

            GetWebsiteConfiguration function = new GetWebsiteConfiguration(
                configurationLoaderMock.Object,
                httpResponseDataFactoryMock.Object,
                websiteConfigurationFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new HttpRequestDataImplementation());

            // Assert
            configurationLoaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateResponse(It.IsAny<HttpRequestData>(), TestData.WebsiteConfiguration));
        }
    }
}
