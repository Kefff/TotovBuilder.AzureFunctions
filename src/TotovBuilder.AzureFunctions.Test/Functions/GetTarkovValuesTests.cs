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
    /// Represents tests on the <see cref="GetTarkovValues"/> class.
    /// </summary>
    public class GetTarkovValuesTests
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

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new Mock<ITarkovValuesFetcher>();
            tarkovValuesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult(TestData.TarkovValues));

            GetTarkovValues function = new GetTarkovValues(configurationLoaderMock.Object, httpResponseDataFactoryMock.Object, tarkovValuesFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new HttpRequestDataImplementation());

            // Assert
            configurationLoaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateResponse(It.IsAny<HttpRequestData>(), TestData.TarkovValues));
        }
    }
}
