using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Test.Mocks;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetItemCategories"/> class.
    /// </summary>
    public class GetItemCategoriesTest
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

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            GetItemCategories function = new(azureFunctionsConfigurationReaderMock.Object, httpResponseDataFactoryMock.Object, itemCategoriesFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new HttpRequestDataImplementation());

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateResponse(It.IsAny<HttpRequestData>(), TestData.ItemCategories));
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();

            Mock<IHttpResponseDataFactory> httpResponseDataFactoryMock = new();
            httpResponseDataFactoryMock
                .Setup(m => m.CreateResponse(It.IsAny<HttpRequestData>(), It.IsAny<object>()))
                .Returns(Task.FromResult((HttpResponseData)new Mock<HttpResponseDataImplementation>().Object));

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(null));

            GetItemCategories function = new(azureFunctionsConfigurationReaderMock.Object, httpResponseDataFactoryMock.Object, itemCategoriesFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new HttpRequestDataImplementation());

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateResponse(It.IsAny<HttpRequestData>(), Array.Empty<ItemCategory>()));
        }
    }
}
