using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Test.Mocks;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetChangelog"/> class.
    /// </summary>
    public class GetChangelogTest
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();

            Mock<IHttpResponseDataFactory> httpResponseDataFactoryMock = new();
            httpResponseDataFactoryMock
                .Setup(m => m.CreateEnumerableResponse(It.IsAny<HttpRequestData>(), It.IsAny<IEnumerable<object>>()))
                .Returns(Task.FromResult((HttpResponseData)new Mock<HttpResponseDataImplementation>().Object));

            Mock<IChangelogFetcher> changelogFetcherMock = new();
            changelogFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ChangelogEntry>>(TestData.Changelog));

            GetChangelog function = new(azureFunctionsConfigurationReaderMock.Object, httpResponseDataFactoryMock.Object, changelogFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new Mock<HttpRequestDataImplementation>().Object);

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateEnumerableResponse(It.IsAny<HttpRequestData>(), TestData.Changelog));
        }
    }
}
