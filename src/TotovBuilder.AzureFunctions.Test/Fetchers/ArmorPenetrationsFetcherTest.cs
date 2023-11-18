﻿using System;
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
    /// Represents tests on the <see cref="ArmorPenetrationsFetcher"/> class.
    /// </summary>
    public class ArmorPenetrationsFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnArmorPenetration()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureArmorPenetrationsBlobName = "armor-penetrations.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ArmorPenetrationsJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            ArmorPenetrationsFetcher fetcher = new ArmorPenetrationsFetcher(
                new Mock<ILogger<ArmorPenetrationsFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object,
                cacheMock.Object);

            // Act
            IEnumerable<ArmorPenetration>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.ArmorPenetrations);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldThrow()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureArmorPenetrationsBlobName = "armor-penetrations.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"[
  {
    invalid
  },
  {
    ""id"": ""5d6e6772a4b936088465b17c"",
    ""caption"": ""12/70 5.25mm Buckshot"",
    ""armorPenetrations"": [3, 3, 3, 3, 3, 3]
  }
]
")));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<ArmorPenetration>>(It.IsAny<DataType>())).Returns(value: null);

            ArmorPenetrationsFetcher fetcher = new ArmorPenetrationsFetcher(
                new Mock<ILogger<ArmorPenetrationsFetcher>>().Object,
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
