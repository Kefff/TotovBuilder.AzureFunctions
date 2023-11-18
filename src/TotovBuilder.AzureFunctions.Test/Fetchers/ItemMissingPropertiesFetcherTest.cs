using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Cache;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Cache;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="ItemMissingPropertiesFetcher"/> class.
    /// </summary>
    public class ItemMissingPropertiesFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnItemMissingProperties()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureItemMissingPropertiesBlobName = "item-missing-properties.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ItemMissingPropertiesJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            ItemMissingPropertiesFetcher fetcher = new ItemMissingPropertiesFetcher(
                new Mock<ILogger<ItemMissingPropertiesFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object,
                cacheMock.Object);

            // Act
            IEnumerable<ItemMissingProperties>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.ItemMissingProperties);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnNull()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureItemMissingPropertiesBlobName = "item-missing-properties.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"[
  {
    invalid
  },
  {
    ""acceptedAmmunitionIds"": [],
    ""conflictingItemIds"": [
      ""5c0e66e2d174af02a96252f4"",
      ""5c0696830db834001d23f5da"",
      ""5c066e3a0db834001b7353f0"",
      ""5c0558060db834001b735271"",
      ""57235b6f24597759bf5a30f1"",
      ""5c110624d174af029e69734c"",
      ""5a16b8a9fcdbcb00165aa6ca""
    ],
    ""id"": ""5a16b7e1fcdbcb00165aa6c9"",
    ""maxStackableAmount"": 1,
    ""modSlots"": []
  }")));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<ItemMissingProperties>>(It.IsAny<DataType>())).Returns(value: null);

            ItemMissingPropertiesFetcher fetcher = new ItemMissingPropertiesFetcher(
                new Mock<ILogger<ItemMissingPropertiesFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object,
                cacheMock.Object);

            // Act
            Func<Task> act = () => fetcher.Fetch();

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }
    }
}
