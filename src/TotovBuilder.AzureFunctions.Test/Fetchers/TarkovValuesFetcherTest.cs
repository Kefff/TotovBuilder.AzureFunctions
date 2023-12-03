using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Abstractions.Utils;
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
                RawTarkovValuesBlobName = "tarkov-values.json"
            });

            Mock<IAzureBlobManager> blobDataFetcherMock = new Mock<IAzureBlobManager>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(TestData.TarkovValuesJson)));

            TarkovValuesFetcher fetcher = new TarkovValuesFetcher(
                new Mock<ILogger<TarkovValuesFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperWrapper.Object);

            // Act
            Result<TarkovValues> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(TestData.TarkovValues);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new Mock<IConfigurationWrapper>();
            configurationWrapperMock.SetupGet(m => m.Values).Returns(new AzureFunctionsConfiguration()
            {
                RawTarkovValuesBlobName = "tarkov-values.json"
            });

            Mock<IAzureBlobManager> blobDataFetcherMock = new Mock<IAzureBlobManager>();
            blobDataFetcherMock.Setup(m => m.Fetch(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(@"{
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

            TarkovValuesFetcher fetcher = new TarkovValuesFetcher(
                new Mock<ILogger<TarkovValuesFetcher>>().Object,
                blobDataFetcherMock.Object,
                configurationWrapperMock.Object);

            // Act
            Result<TarkovValues> result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("TarkovValues - No data fetched.");
        }
    }
}
