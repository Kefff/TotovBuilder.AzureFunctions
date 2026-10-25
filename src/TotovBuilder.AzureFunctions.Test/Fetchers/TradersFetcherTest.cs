using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Test;
using TotovBuilder.Model.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="TradersFetcher"/> class.
    /// </summary>
    public class TradersFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldFetchMerchantNames()
        {
            // Arrange
            TradersFetcher fetcher = CreateFetcher(ApiRequesterMock.CreateForPricesAndBarters());

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().ContainKey("54cb50c76803fa8b248b4571").WhoseValue.Should().Be("prapor");
            fetcher.GetMerchantName("5935c25fb3acc3127c3d8cd9").Should().Be("peacekeeper");
        }

        [Fact]
        public async Task GetMerchantName_WithUnknownTrader_ShouldThrow()
        {
            // Arrange
            TradersFetcher fetcher = CreateFetcher(ApiRequesterMock.CreateForPricesAndBarters());
            await fetcher.Fetch();

            // Act
            Action act = () => fetcher.GetMerchantName("unknown-trader");

            // Assert
            act.Should().Throw<InvalidDataException>().WithMessage("Trader \"unknown-trader\" not found.");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Fetch_WithFailedOrInvalidTradersFile_ShouldFail(bool isFileFound)
        {
            // Arrange
            Dictionary<string, string> dataJsonByPath = [];

            if (isFileFound)
            {
                dataJsonByPath.Add("/regular/traders", @"{ ""data"": [] }");
            }

            TradersFetcher fetcher = CreateFetcher(ApiRequesterMock.Create(dataJsonByPath, []));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Traders - No data fetched.");
        }

        private static TradersFetcher CreateFetcher(Mock<IApiRequester> apiRequesterMock)
        {
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration);

            return new TradersFetcher(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                new Mock<ILogger<TradersFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object);
        }
    }
}
