using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Abstractions.Wrappers;
using TotovBuilder.AzureFunctions.Fetchers;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using TotovBuilder.Model.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="GameModeLocalizedPricesFetcher"/> class.
    /// </summary>
    public class GameModeLocalizedPricesFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldFetchPricesAllGameModesAndAllLanguages()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = CreateApiRequesterMock();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                    .Returns(Task.FromResult(Result.Ok()))
                    .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                    .Returns(TestData.TarkovValues)
                    .Verifiable();

            GameModeLocalizedPricesFetcher gameModeLocalizedPricesFetcher = new(
                new Mock<ILogger<PricesFetcher>>().Object,
                new Mock<ILogger<BartersFetcher>>().Object,
                new Mock<ILogger<QuestsFetcher>>().Object,
                new Mock<ILogger<TradersFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                tarkovValuesFetcherMock.Object,
                CreateTimeProvider());

            // Act
            Result result = await gameModeLocalizedPricesFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(0).GameMode.Name.Should().Be("pvp");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(0).Language.Should().Be("en");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(0).Prices.Should().BeEquivalentTo(PricesPvpEN);
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(1).GameMode.Name.Should().Be("pvp");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(1).Language.Should().Be("fr");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(1).Prices.Should().BeEquivalentTo(PricesPvpFR);
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(2).GameMode.Name.Should().Be("pve");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(2).Language.Should().Be("en");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(2).Prices.Should().BeEquivalentTo(PricesPveEN);
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(3).GameMode.Name.Should().Be("pve");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(3).Language.Should().Be("fr");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(3).Prices.Should().BeEquivalentTo(PricesPveFR);
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(4).GameMode.Name.Should().Be("pvp-season");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(4).Language.Should().Be("en");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(4).Prices.Should().BeEquivalentTo(PricesPvpEN);
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(5).GameMode.Name.Should().Be("pvp-season");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(5).Language.Should().Be("fr");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(5).Prices.Should().BeEquivalentTo(PricesPvpFR);

            apiRequesterMock.Verify(m => m.RequestData("/regular/items"), Times.Exactly(2));
            apiRequesterMock.Verify(m => m.RequestData("/pve/items"), Times.Exactly(2));
        }

        [Fact]
        public async Task Fetch_WithAlreadyFetchedData_ShouldDoNothing()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = CreateApiRequesterMock();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            GameModeLocalizedPricesFetcher gameModeLocalizedPricesFetcher = new(
                new Mock<ILogger<PricesFetcher>>().Object,
                new Mock<ILogger<BartersFetcher>>().Object,
                new Mock<ILogger<QuestsFetcher>>().Object,
                new Mock<ILogger<TradersFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                tarkovValuesFetcherMock.Object,
                CreateTimeProvider());

            // Act
            await gameModeLocalizedPricesFetcher.Fetch();
            Result result = await gameModeLocalizedPricesFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(0).GameMode.Name.Should().Be("pvp");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(0).Language.Should().Be("en");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(0).Prices.Should().BeEquivalentTo(PricesPvpEN);
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(1).GameMode.Name.Should().Be("pvp");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(1).Language.Should().Be("fr");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(1).Prices.Should().BeEquivalentTo(PricesPvpFR);
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(2).GameMode.Name.Should().Be("pve");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(2).Language.Should().Be("en");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(2).Prices.Should().BeEquivalentTo(PricesPveEN);
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(3).GameMode.Name.Should().Be("pve");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(3).Language.Should().Be("fr");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(3).Prices.Should().BeEquivalentTo(PricesPveFR);
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(4).GameMode.Name.Should().Be("pvp-season");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(4).Language.Should().Be("en");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(4).Prices.Should().BeEquivalentTo(PricesPvpEN);
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(5).GameMode.Name.Should().Be("pvp-season");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(5).Language.Should().Be("fr");
            gameModeLocalizedPricesFetcher.FetchedData!.ElementAt(5).Prices.Should().BeEquivalentTo(PricesPvpFR);

            apiRequesterMock.Verify(m => m.RequestData("/regular/items"), Times.Exactly(2));
            apiRequesterMock.Verify(m => m.RequestData("/pve/items"), Times.Exactly(2));
        }

        [Theory]
        [InlineData(-1, 1, true, true)]
        [InlineData(-2, -1, true, false)]
        [InlineData(1, 2, true, false)]
        [InlineData(-1, 1, false, false)]
        [InlineData(null, null, true, false)]
        public async Task Fetch_WithFallbackGameMode_ShouldUseSeasonPricesOnlyWhenSeasonIsInProgress(int? startOffsetInDays, int? endOffsetInDays, bool hasSeasonFiles, bool expectedSeasonPrices)
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            string? seasonJson = startOffsetInDays == null
                ? null
                : $@"{{
  ""data"": {{
    ""start"": {Now.AddDays(startOffsetInDays.Value).ToUnixTimeMilliseconds()},
    ""end"": {Now.AddDays(endOffsetInDays!.Value).ToUnixTimeMilliseconds()}
  }}
}}";
            Mock<IApiRequester> apiRequesterMock = CreateApiRequesterMock(seasonJson, hasSeasonFiles);

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            Mock<ILogger<PricesFetcher>> pricesFetcherLoggerMock = new();

            GameModeLocalizedPricesFetcher gameModeLocalizedPricesFetcher = new(
                pricesFetcherLoggerMock.Object,
                new Mock<ILogger<BartersFetcher>>().Object,
                new Mock<ILogger<QuestsFetcher>>().Object,
                new Mock<ILogger<TradersFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                tarkovValuesFetcherMock.Object,
                CreateTimeProvider());

            // Act
            Result result = await gameModeLocalizedPricesFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();

            GameModeLocalizedPrices seasonPrices = gameModeLocalizedPricesFetcher.FetchedData!.Single(fd => fd.GameMode.Name == "pvp-season" && fd.Language == "en");
            seasonPrices.Prices.Should().BeEquivalentTo(expectedSeasonPrices ? PricesSeasonEN : PricesPvpEN);
            gameModeLocalizedPricesFetcher.FetchedData!.Count(fd => fd.GameMode.Name == "pvp-season").Should().Be(2);

            pricesFetcherLoggerMock.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("pvp-season - Prices not available")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                expectedSeasonPrices ? Times.Never : Times.Once);
        }

        [Fact]
        public async Task Fetch_WithNoDataFetched_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.CreateForPricesAndBarters(@"{
  ""data"": {
    ""items"": {}
  }
}");

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            GameModeLocalizedPricesFetcher gameModeLocalizedPricesFetcher = new(
                new Mock<ILogger<PricesFetcher>>().Object,
                new Mock<ILogger<BartersFetcher>>().Object,
                new Mock<ILogger<QuestsFetcher>>().Object,
                new Mock<ILogger<TradersFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                tarkovValuesFetcherMock.Object,
                CreateTimeProvider());

            // Act
            Result result = await gameModeLocalizedPricesFetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Prices - No data fetched.");
            gameModeLocalizedPricesFetcher.FetchedData.Should().BeNull();
        }

        private static readonly Price[] PricesPvpEN = [
            new()
            {
                CurrencyName = "USD",
                ItemId = "58948c8e86f77409493f7266",
                Merchant = "peacekeeper",
                MerchantLevel = 2,
                Quest = new Quest()
                {
                    Id = "5a27b80086f774429a5d7e20",
                    Name = "Eagle Eye",
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Eagle_Eye"
                },
                Value = 539,
                ValueInMainCurrency = 64141
            },
            new()
            {
                CurrencyName = "RUB",
                ItemId = "5449016a4bdc2d6f028b456f",
                Merchant = "prapor",
                MerchantLevel = 1,
                Value = 1,
                ValueInMainCurrency = 1
            }
        ];

        private const string ItemsJsonPvp = @"{
  ""data"": {
    ""fleaMarket"": {
      ""enabled"": true,
      ""normalizedName"": ""flea-market""
    },
    ""items"": {
      ""58948c8e86f77409493f7266"": {
        ""buyFromTrader"": [
          {
            ""currency"": ""USD"",
            ""minTraderLevel"": 2,
            ""price"": 539,
            ""priceRUB"": 64141,
            ""taskUnlock"": ""5a27b80086f774429a5d7e20"",
            ""trader"": ""5935c25fb3acc3127c3d8cd9""
          }
        ],
        ""id"": ""58948c8e86f77409493f7266"",
        ""types"": [
          ""noFlea""
        ]
      }
    }
  }
}";

        private static readonly Price[] PricesPvpFR = [
            new()
            {
                CurrencyName = "USD",
                ItemId = "58948c8e86f77409493f7266",
                Merchant = "peacekeeper",
                MerchantLevel = 2,
                Quest = new Quest()
                {
                    Id = "5a27b80086f774429a5d7e20",
                    Name = "Oeil d'aigle",
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Eagle_Eye"
                },
                Value = 539,
                ValueInMainCurrency = 64141
            },
            new()
            {
                CurrencyName = "RUB",
                ItemId = "5449016a4bdc2d6f028b456f",
                Merchant = "prapor",
                MerchantLevel = 1,
                Value = 1,
                ValueInMainCurrency = 1
            }
        ];

        private const string TasksTranslationsJsonFR = @"{
  ""data"": {
    ""5a27b80086f774429a5d7e20 name"": ""Oeil d'aigle""
  }
}";

        private static readonly Price[] PricesPveEN = [
            new()
            {
                CurrencyName = "USD",
                ItemId = "58948c8e86f77409493f7266",
                Merchant = "peacekeeper",
                MerchantLevel = 2,
                Quest = new Quest()
                {
                    Id = "5a27b80086f774429a5d7e20",
                    Name = "Eagle Eye",
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Eagle_Eye"
                },
                Value = 270,
                ValueInMainCurrency = 32070
            },
            new()
            {
                CurrencyName = "RUB",
                ItemId = "5449016a4bdc2d6f028b456f",
                Merchant = "prapor",
                MerchantLevel = 1,
                Value = 1,
                ValueInMainCurrency = 1
            }
        ];

        private const string ItemsJsonPve = @"{
  ""data"": {
    ""fleaMarket"": {
      ""enabled"": true,
      ""normalizedName"": ""flea-market""
    },
    ""items"": {
      ""58948c8e86f77409493f7266"": {
        ""buyFromTrader"": [
          {
            ""currency"": ""USD"",
            ""minTraderLevel"": 2,
            ""price"": 270,
            ""priceRUB"": 32070,
            ""taskUnlock"": ""5a27b80086f774429a5d7e20"",
            ""trader"": ""5935c25fb3acc3127c3d8cd9""
          }
        ],
        ""id"": ""58948c8e86f77409493f7266"",
        ""types"": [
          ""noFlea""
        ]
      }
    }
  }
}";

        private static readonly Price[] PricesPveFR = [
            new()
            {
                CurrencyName = "USD",
                ItemId = "58948c8e86f77409493f7266",
                Merchant = "peacekeeper",
                MerchantLevel = 2,
                Quest = new Quest()
                {
                    Id = "5a27b80086f774429a5d7e20",
                    Name = "Oeil d'aigle",
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Eagle_Eye"
                },
                Value = 270,
                ValueInMainCurrency = 32070
            },
            new()
            {
                CurrencyName = "RUB",
                ItemId = "5449016a4bdc2d6f028b456f",
                Merchant = "prapor",
                MerchantLevel = 1,
                Value = 1,
                ValueInMainCurrency = 1
            }
        ];

        private static readonly DateTimeOffset Now = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

        private static readonly Price[] PricesSeasonEN = [
            new()
            {
                CurrencyName = "USD",
                ItemId = "58948c8e86f77409493f7266",
                Merchant = "peacekeeper",
                MerchantLevel = 2,
                Quest = new Quest()
                {
                    Id = "5a27b80086f774429a5d7e20",
                    Name = "Eagle Eye",
                    WikiLink = "https://escapefromtarkov.fandom.com/wiki/Eagle_Eye"
                },
                Value = 300,
                ValueInMainCurrency = 35000
            },
            new()
            {
                CurrencyName = "RUB",
                ItemId = "5449016a4bdc2d6f028b456f",
                Merchant = "prapor",
                MerchantLevel = 1,
                Value = 1,
                ValueInMainCurrency = 1
            }
        ];

        private static Mock<IApiRequester> CreateApiRequesterMock(string? seasonJson = null, bool hasSeasonFiles = false)
        {
            const string bartersJson = @"{
  ""data"": []
}";

            Dictionary<string, string> dataJsonByPath = new()
            {
                { "/pve/barters", bartersJson },
                { "/pve/items", ItemsJsonPve },
                { "/pve/tasks", TestData.TasksJson },
                { "/pve/traders", TestData.TradersJson },
                { "/regular/barters", bartersJson },
                { "/regular/items", ItemsJsonPvp },
                { "/regular/tasks", TestData.TasksJson },
                { "/regular/traders", TestData.TradersJson }
            };
            Dictionary<string, string> translationsJsonByPath = new()
            {
                { "/pve/tasks_en", TestData.TasksTranslationsJson },
                { "/pve/tasks_fr", TasksTranslationsJsonFR },
                { "/regular/tasks_en", TestData.TasksTranslationsJson },
                { "/regular/tasks_fr", TasksTranslationsJsonFR }
            };

            if (seasonJson != null)
            {
                dataJsonByPath.Add("/pvp-season/season", seasonJson);
            }

            if (hasSeasonFiles)
            {
                dataJsonByPath.Add("/pvp-season/barters", bartersJson);
                dataJsonByPath.Add("/pvp-season/items", ItemsJsonPvp.Replace("539", "300").Replace("64141", "35000"));
                dataJsonByPath.Add("/pvp-season/tasks", TestData.TasksJson);
                dataJsonByPath.Add("/pvp-season/traders", TestData.TradersJson);
                translationsJsonByPath.Add("/pvp-season/tasks_en", TestData.TasksTranslationsJson);
                translationsJsonByPath.Add("/pvp-season/tasks_fr", TasksTranslationsJsonFR);
            }

            return ApiRequesterMock.Create(dataJsonByPath, translationsJsonByPath);
        }

        private static TimeProvider CreateTimeProvider()
        {
            Mock<TimeProvider> timeProviderMock = new();
            timeProviderMock
                .Setup(m => m.GetUtcNow())
                .Returns(Now);

            return timeProviderMock.Object;
        }
    }
}
