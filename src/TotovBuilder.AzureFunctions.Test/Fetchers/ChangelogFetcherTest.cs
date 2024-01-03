using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using TotovBuilder.Shared.Abstractions.Azure;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="ChangelogFetcher"/> class.
    /// </summary>
    public class ChangelogFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturn5LastChangelogs()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                RawChangelogBlobName = "changelog.json"
            });

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new Mock<IAzureBlobStorageManager>();
            azureBlobStorageManagerMock.Setup(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ChangelogJson)));

            ChangelogFetcher fetcher = new ChangelogFetcher(
                new Mock<ILogger<ChangelogFetcher>>().Object,
                azureBlobStorageManagerMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<ChangelogEntry>> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(new ChangelogEntry[]
            {
                new ChangelogEntry()
                {
                    Changes = new ChangelogChange[]
                    {
                        new ChangelogChange()
                        {
                            Language = "en",
                            Text = "Added a thing."
                        },
                        new ChangelogChange()
                        {
                            Language = "fr",
                            Text = "Ajout d'une chose."
                        }
                    },
                    Date = new DateTime(2022, 1, 2),
                    Version = "1.5.0",
                },
                new ChangelogEntry()
                {
                    Changes = new ChangelogChange[]
                    {
                        new ChangelogChange()
                        {
                            Language = "en",
                            Text = "Added a thing."
                        },
                        new ChangelogChange()
                        {
                            Language = "fr",
                            Text = "Ajout d'une chose."
                        }
                    },
                    Date = new DateTime(2022, 1, 2),
                    Version = "1.4.0",
                },
                new ChangelogEntry()
                {
                    Changes = new ChangelogChange[]
                    {
                        new ChangelogChange()
                        {
                            Language = "en",
                            Text = "Added a thing."
                        },
                        new ChangelogChange()
                        {
                            Language = "fr",
                            Text = "Ajout d'une chose."
                        }
                    },
                    Date = new DateTime(2022, 1, 2),
                    Version = "1.3.0",
                },
                new ChangelogEntry()
                {
                    Changes = new ChangelogChange[]
                    {
                        new ChangelogChange()
                        {
                            Language = "en",
                            Text = "Added a thing."
                        },
                        new ChangelogChange()
                        {
                            Language = "fr",
                            Text = "Ajout d'une chose."
                        }
                    },
                    Date = new DateTime(2022, 1, 2),
                    Version = "1.2.0",
                },
                new ChangelogEntry()
                {
                    Changes = new ChangelogChange[]
                    {
                        new ChangelogChange()
                        {
                            Language = "en",
                            Text = "Added a thing."
                        },
                        new ChangelogChange()
                        {
                            Language = "fr",
                            Text = "Ajout d'une chose."
                        }
                    },
                    Date = new DateTime(2022, 1, 2),
                    Version = "1.1.0",
                }
            });
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                RawChangelogBlobName = "changelog.json"
            });

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new Mock<IAzureBlobStorageManager>();
            azureBlobStorageManagerMock.Setup(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"[
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

            ChangelogFetcher fetcher = new ChangelogFetcher(
                new Mock<ILogger<ChangelogFetcher>>().Object,
                azureBlobStorageManagerMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<ChangelogEntry>> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Changelog - No data fetched.");
        }
    }
}
