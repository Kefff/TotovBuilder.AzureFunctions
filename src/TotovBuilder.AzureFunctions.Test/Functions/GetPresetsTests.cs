using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Test.Net.Mocks;
using TotovBuilder.Model.Builds;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetPresets"/> class.
    /// </summary>
    public class GetPresetsTests
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            Mock<IConfigurationLoader> configurationLoaderMock = new Mock<IConfigurationLoader>();

            Mock<IHttpResponseDataFactory> httpResponseDataFactoryMock = new Mock<IHttpResponseDataFactory>();
            httpResponseDataFactoryMock
                .Setup(m => m.CreateEnumerableResponse(It.IsAny<HttpRequestData>(), It.IsAny<IEnumerable<object>>()))
                .Returns(Task.FromResult((HttpResponseData)new Mock<HttpResponseDataImplementation>().Object));

            Mock<IPresetsFetcher> presetsFetcherMock = new Mock<IPresetsFetcher>();
            presetsFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<InventoryItem>>(TestData.Presets));

            GetPresets function = new GetPresets(configurationLoaderMock.Object, httpResponseDataFactoryMock.Object, presetsFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new HttpRequestDataImplementation());

            // Assert
            configurationLoaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateEnumerableResponse(It.IsAny<HttpRequestData>(), TestData.Presets));
        }
    }
}
