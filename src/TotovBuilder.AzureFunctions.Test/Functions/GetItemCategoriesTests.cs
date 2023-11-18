using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Net;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Test.Net.Mocks;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetItemCategories"/> class.
    /// </summary>
    public class GetItemCategoriesTests
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

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>>(TestData.ItemCategories));

            GetItemCategories function = new GetItemCategories(configurationLoaderMock.Object, httpResponseDataFactoryMock.Object, itemCategoriesFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new HttpRequestDataImplementation());

            // Assert
            IEnumerable<string> expected = TestData.ItemCategories.Select(c => c.Id);
            configurationLoaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateEnumerableResponse(It.IsAny<HttpRequestData>(), expected));
        }
    }
}
