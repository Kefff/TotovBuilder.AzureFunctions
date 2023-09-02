﻿using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Test.Mocks;
using TotovBuilder.Model.Test;
using Xunit;

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
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();

            Mock<IHttpResponseDataFactory> httpResponseDataFactoryMock = new();
            httpResponseDataFactoryMock
                .Setup(m => m.CreateResponse(It.IsAny<HttpRequestData>(), It.IsAny<object>()))
                .Returns(Task.FromResult((HttpResponseData)new Mock<HttpResponseDataImplementation>().Object));

            Mock<IWebsiteConfigurationFetcher> websiteConfigurationFetcherMock = new();
            websiteConfigurationFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(TestData.WebsiteConfiguration));

            GetWebsiteConfiguration function = new(
                azureFunctionsConfigurationReaderMock.Object,
                httpResponseDataFactoryMock.Object,
                websiteConfigurationFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new HttpRequestDataImplementation());

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateResponse(It.IsAny<HttpRequestData>(), TestData.WebsiteConfiguration));
        }
    }
}
