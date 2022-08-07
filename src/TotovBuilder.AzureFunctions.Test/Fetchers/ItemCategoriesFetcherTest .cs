using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Items;
using Xunit;
using TotovBuilder.Model;
using TotovBuilder.Model.Test;

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
            Mock<ILogger<ItemCategoriesFetcher>> loggerMock = new Mock<ILogger<ItemCategoriesFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureItemCategoriesBlobName = "item-categories.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ItemCategoriesJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            ItemCategoriesFetcher fetcher = new ItemCategoriesFetcher(loggerMock.Object, blobDataFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<ItemCategory>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.ItemCategories);
        }

        [Fact]
        public async void Fetch_WithInvalidData_ShouldReturnNull()
        {
            // Arrange
            Mock<ILogger<ItemCategoriesFetcher>> loggerMock = new Mock<ILogger<ItemCategoriesFetcher>>();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureItemCategoriesBlobName = "item-categories.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"[
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

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<ItemCategory>>(It.IsAny<DataType>())).Returns(value: null);

            ItemCategoriesFetcher fetcher = new ItemCategoriesFetcher(loggerMock.Object, blobDataFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            IEnumerable<ItemCategory>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeNull();
        }
    }
}
