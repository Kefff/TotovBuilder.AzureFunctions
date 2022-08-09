using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.Model.Items;
using Xunit;
using TotovBuilder.Model.Test;
using TotovBuilder.AzureFunctions.Abstractions;

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
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            GetItemCategories function = new GetItemCategories(azureFunctionsConfigurationReaderMock.Object, itemCategoriesFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().Be(TestData.ItemCategories);
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();

            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(null));

            GetItemCategories function = new GetItemCategories(azureFunctionsConfigurationReaderMock.Object, itemCategoriesFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(Array.Empty<ItemCategory>());
        }
    }
}
