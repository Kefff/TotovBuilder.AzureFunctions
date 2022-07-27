using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Models;
using TotovBuilder.AzureFunctions.Test.Mocks;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test
{
    /// <summary>
    /// Represents tests on the <see cref="ItemCategoryFinder"/> class.
    /// </summary>
    public class ItemCategoryFinderTest
    {
        [Theory]
        [InlineData("5485a8684bdc2da71d8b4567", "ammunition")]
        [InlineData("5c164d2286f774194c5e69fa", "other")]
        public async Task FindFromTarkovCategoryIds_ShouldGetItemCategory(string tarkovItemCategoryId, string expected)
        {
            // Arrange
            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            ItemCategoryFinder itemCategoryFinder = new ItemCategoryFinder(itemCategoriesFetcherMock.Object);

            // Act
            ItemCategory itemCategory = await itemCategoryFinder.FindFromTarkovCategoryId(tarkovItemCategoryId);

            // Assert
            itemCategory.Id.Should().Be(expected);
        }

        [Fact]
        public async Task FindFromTarkovCategoryIds_WithItemCategoryFetchingError_ShouldReturnOtherItemCategory()
        {
            // Arrange
            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(null));

            ItemCategoryFinder itemCategoryFinder = new ItemCategoryFinder(itemCategoriesFetcherMock.Object);

            // Act
            ItemCategory itemCategory = await itemCategoryFinder.FindFromTarkovCategoryId("5485a8684bdc2da71d8b4567");

            // Assert
            itemCategory.Id.Should().Be("other");
        }
    }
}
