using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.AzureFunctions.Utils;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Test;
using TotovBuilder.Shared.Abstractions.Azure;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="StaticDataFetcher"/> class.
    /// </summary>
    public class RawDataFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldFetchedData()
        {
            // Arrange
            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new();
            azureBlobStorageManagerMock.Setup(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ChangelogJson)));

            Mock<IConfigurationWrapper> mockConfigurationWrapperMock = new();
            mockConfigurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration());

            RawDataFetcherImplementation rawDataFetcher = new(
                new Mock<ILogger<RawDataFetcherImplementation>>().Object,
                azureBlobStorageManagerMock.Object,
                mockConfigurationWrapperMock.Object);

            // Act
            Result<IEnumerable<ChangelogEntry>> result = await rawDataFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.Changelog);
            azureBlobStorageManagerMock.Verify(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithPreviousFetchingTask_ShouldWaitForItToEndAndReturnFetchedData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                RawChangelogBlobName = "changelog.json"
            });

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new();
            azureBlobStorageManagerMock
                .Setup(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    return Result.Ok(TestData.ChangelogJson);
                });

            RawDataFetcherImplementation rawDataFetcher = new(
                new Mock<ILogger<RawDataFetcherImplementation>>().Object,
                azureBlobStorageManagerMock.Object,
                configurationWrapperMock.Object);

            // Act
            _ = rawDataFetcher.Fetch();
            Result<IEnumerable<ChangelogEntry>> result = await rawDataFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.Changelog);
            azureBlobStorageManagerMock.Verify(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithAlreadyFetchedData_ShouldReturnFetchedData()
        {
            // Arrange
            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new();
            azureBlobStorageManagerMock.Setup(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.ChangelogJson)));

            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration());

            RawDataFetcherImplementation rawDataFetcher = new(
                new Mock<ILogger<RawDataFetcherImplementation>>().Object,
                azureBlobStorageManagerMock.Object,
                configurationWrapperMock.Object);

            // Act
            await rawDataFetcher.Fetch();
            Result<IEnumerable<ChangelogEntry>> result = await rawDataFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.Changelog);
            azureBlobStorageManagerMock.Verify(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Fetch_WithError_ShouldFail()
        {
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                RawChangelogBlobName = "changelog.json"
            });

            Mock<IAzureBlobStorageManager> azureBlobStorageManagerMock = new();
            azureBlobStorageManagerMock.Setup(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Fail<string>(string.Empty)));

            RawDataFetcherImplementation rawDataFetcher = new(
                new Mock<ILogger<RawDataFetcherImplementation>>().Object,
                azureBlobStorageManagerMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<IEnumerable<ChangelogEntry>> result = await rawDataFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Changelog - No data fetched.");
            azureBlobStorageManagerMock.Verify(m => m.FetchBlob(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        public class RawDataFetcherImplementation : RawDataFetcher<IEnumerable<ChangelogEntry>>
        {
            protected override string AzureBlobName
            {
                get
                {
                    return ConfigurationWrapper.Values.RawChangelogBlobName;
                }
            }

            protected override DataType DataType
            {
                get
                {
                    return DataType.Changelog;
                }
            }

            public RawDataFetcherImplementation(
                ILogger<RawDataFetcherImplementation> logger,
                IAzureBlobStorageManager azureBlobStorageManager,
                IConfigurationWrapper configurationWrapper)
               : base(logger, azureBlobStorageManager, configurationWrapper)
            {
            }

            protected override Task<Result<IEnumerable<ChangelogEntry>>> DeserializeData(string responseContent)
            {
                return Task.FromResult(Result.Ok(TestData.Changelog.AsEnumerable()));
            }
        }
    }
}
