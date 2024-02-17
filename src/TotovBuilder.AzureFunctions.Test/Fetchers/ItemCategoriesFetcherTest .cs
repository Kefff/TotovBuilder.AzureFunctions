using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using TotovBuilder.Shared.Abstractions.Azure;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="ItemCategoriesFetcher"/> class.
    /// </summary>
    public class ItemCategoriesFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnItemCategories()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                RawItemCategoriesBlobName = "item-categories.json"
            });

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new Mock<IAzureBlobStorageManager>();
            azureBlobStorageManagerMock.Setup(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ItemCategoriesJson)));

            ItemCategoriesFetcher fetcher = new ItemCategoriesFetcher(
                new Mock<ILogger<ItemCategoriesFetcher>>().Object,
                azureBlobStorageManagerMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<ItemCategory>> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.ItemCategories);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                RawItemCategoriesBlobName = "item-categories.json"
            });

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new Mock<IAzureBlobStorageManager>();
            azureBlobStorageManagerMock.Setup(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"[
  {
    invalid
  },
  {
    ""id"": ""ammunition"",
    ""types"": [
      {
        ""id"": ""5485a8684bdc2da71d8b4567"",
        ""name"": ""Ammo""
      }
    ]
  }
]
")));

            ItemCategoriesFetcher fetcher = new ItemCategoriesFetcher(
                new Mock<ILogger<ItemCategoriesFetcher>>().Object,
                azureBlobStorageManagerMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<ItemCategory>> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("ItemCategories - No data fetched.");
        }
    }
}
