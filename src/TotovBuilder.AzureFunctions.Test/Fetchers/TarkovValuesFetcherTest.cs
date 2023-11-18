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
    /// Represents tests on the <see cref="TarkovValuesFetcher"/> class.
    /// </summary>
    public class TarkovValuesFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnTarkovValues()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperWrapper = new Mock<IConfigurationWrapper>();
            configurationWrapperWrapper.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureTarkovValuesBlobName = "tarkov-values.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.TarkovValuesJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            TarkovValuesFetcher fetcher = new TarkovValuesFetcher(
                new Mock<ILogger<TarkovValuesFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperWrapper.Object,
                cacheMock.Object);

            // Act
            TarkovValues? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.TarkovValues);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnNull()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureTarkovValuesBlobName = "tarkov-values.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
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

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<TarkovValues>>(It.IsAny<DataType>())).Returns(value: null);

            TarkovValuesFetcher fetcher = new TarkovValuesFetcher(
                new Mock<ILogger<TarkovValuesFetcher>>().Object,
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
