using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Test.Net.Mocks;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetChangelog"/> class.
    /// </summary>
    public class GetChangelogTests
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

            Mock<IChangelogFetcher> changelogFetcherMock = new Mock<IChangelogFetcher>();
            changelogFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ChangelogEntry>>(TestData.Changelog));

            GetChangelog function = new GetChangelog(configurationLoaderMock.Object, httpResponseDataFactoryMock.Object, changelogFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new Mock<HttpRequestDataImplementation>().Object);

            // Assert
            configurationLoaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateEnumerableResponse(It.IsAny<HttpRequestData>(), TestData.Changelog));
        }
    }
}
