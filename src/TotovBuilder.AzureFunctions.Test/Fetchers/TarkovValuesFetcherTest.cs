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
using TotovBuilder.Model.Test;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="TarkovValuesFetcher"/> class.
    /// </summary>
    public class TarkovValuesFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnTarkovValues()
        {
            // Arrange
            Mock<ILogger<TarkovValuesFetcher>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderWrapper = new();
            azureFunctionsConfigurationReaderWrapper.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureTarkovValuesBlobName = "tarkov-values.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.TarkovValuesJson)));

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            TarkovValuesFetcher fetcher = new(loggerMock.Object, blobDataFetcherMock.Object, azureFunctionsConfigurationReaderWrapper.Object, cacheMock.Object);

            // Act
            TarkovValues? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.TarkovValues);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnNull()
        {
            // Arrange
            Mock<ILogger<TarkovValuesFetcher>> loggerMock = new();

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();
            azureFunctionsConfigurationReaderMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureTarkovValuesBlobName = "tarkov-values.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"{
  ""invalid"": {
    invalid
  },
  ""armorPenetrationEfficiencies"": [
    ""> 20"",
    ""13 - 20"",
    ""9 - 13"",
    ""5 - 9"",
    ""3 - 5"",
    ""1 - 3"",
    ""< 1""
  ]
}
")));

            Mock<ICache> cacheMock = new();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<TarkovValues>>(It.IsAny<DataType>())).Returns(value: null);

            TarkovValuesFetcher fetcher = new(loggerMock.Object, blobDataFetcherMock.Object, azureFunctionsConfigurationReaderMock.Object, cacheMock.Object);

            // Act
            TarkovValues? result = await fetcher.Fetch();

            // Assert
            result.Should().BeNull();
        }
    }
}
