﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model;
using Xunit;
using TotovBuilder.Model.Test;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="ChangelogFetcher"/> class.
    /// </summary>
    public class ChangelogFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnChangelog()
        {
            // Arrange
            Mock<ILogger<ChangelogFetcher>> loggerMock = new Mock<ILogger<ChangelogFetcher>>();

            Mock<IAzureFunctionsConfigurationWrapper> azureFunctionsConfigurationWrapperMock = new Mock<IAzureFunctionsConfigurationWrapper>();
            azureFunctionsConfigurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureChangelogBlobName = "changelog.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ChangelogJson)));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);

            ChangelogFetcher fetcher = new ChangelogFetcher(loggerMock.Object, blobDataFetcherMock.Object, azureFunctionsConfigurationWrapperMock.Object, cacheMock.Object);

            // Act
            IEnumerable<ChangelogEntry>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeEquivalentTo(TestData.Changelog);
        }

        [Fact]
        public async void Fetch_WithInvalidData_ShouldReturnNull()
        {
            // Arrange
            Mock<ILogger<ChangelogFetcher>> loggerMock = new Mock<ILogger<ChangelogFetcher>>();

            Mock<IAzureFunctionsConfigurationWrapper> azureFunctionsConfigurationWrapperMock = new Mock<IAzureFunctionsConfigurationWrapper>();
            azureFunctionsConfigurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                AzureChangelogBlobName = "changelog.json"
            });

            Mock<IBlobFetcher> blobDataFetcherMock = new Mock<IBlobFetcher>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"[
  {
    invalid
  },
  {
    ""version"": ""1.1.0"",
    ""date"": ""2022-01-02T00:00:00+01:00"",
    ""changes"": [
      {
        ""language"": ""en"",
        ""text"": ""Added a thing.""
      },
      {
        ""language"": ""fr"",
        ""text"": ""Ajout d'une chose.""
      }
    ]
  }
]
")));

            Mock<ICache> cacheMock = new Mock<ICache>();
            cacheMock.Setup(m => m.HasValidCache(It.IsAny<DataType>())).Returns(false);
            cacheMock.Setup(m => m.Get<IEnumerable<ChangelogEntry>>(It.IsAny<DataType>())).Returns(value: null);

            ChangelogFetcher fetcher = new ChangelogFetcher(loggerMock.Object, blobDataFetcherMock.Object, azureFunctionsConfigurationWrapperMock.Object, cacheMock.Object);

            // Act
            IEnumerable<ChangelogEntry>? result = await fetcher.Fetch();

            // Assert
            result.Should().BeNull();
        }
    }
}
