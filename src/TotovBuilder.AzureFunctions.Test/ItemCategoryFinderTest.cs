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
        [InlineData("ammunition", "5485a8684bdc2da71d8b4567")]
        [InlineData("other", "5c164d2286f774194c5e69fa", "543be5e94bdc2df1348b4568")]
        public async Task FindFromTarkovCategoryIds_ShouldGetItemCategory(string expected, params string[] tarkovItemCategoryIds)
        {
            // Arrange
            Mock<IItemCategoriesFetcher> itemCategoriesFetcherMock = new Mock<IItemCategoriesFetcher>();
            itemCategoriesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<ItemCategory>?>(TestData.ItemCategories));

            ItemCategoryFinder itemCategoryFinder = new ItemCategoryFinder(itemCategoriesFetcherMock.Object);

            // Act
            ItemCategory itemCategory = await itemCategoryFinder.FindFromTarkovCategoryIds(tarkovItemCategoryIds);

            // Assert
            itemCategory.Id.Should().Be(expected);
        }
    }
}
