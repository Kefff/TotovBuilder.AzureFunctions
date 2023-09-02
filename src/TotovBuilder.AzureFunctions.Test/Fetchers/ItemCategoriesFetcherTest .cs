using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
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
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureItemCategoriesBlobName = "item-categories.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ItemCategoriesJson)));

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            ItemCategoriesFetcher fetcher = new(
                new Mock<ILogger<ItemCategoriesFetcher>>().Object,
                blobDataFetcherMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object);

            // Act
            IEnumerable<ItemCategory>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.ItemCategories);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnNull()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationCache> azureFunctionsConfigurationCacheMock = new();
            azureFunctionsConfigurationCacheMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureItemCategoriesBlobName = "item-categories.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new();
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

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<ItemCategory>>(It.IsAny<DataType>())).Returns(value: null);

            ItemCategoriesFetcher fetcher = new(
                new Mock<ILogger<ItemCategoriesFetcher>>().Object,
                blobDataFetcherMock.Object,
                azureFunctionsConfigurationCacheMock.Object,
                cacheMock.Object);

            // Act
            Func<Task> act = () => fetcher.Fetch();

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }
    }
}
