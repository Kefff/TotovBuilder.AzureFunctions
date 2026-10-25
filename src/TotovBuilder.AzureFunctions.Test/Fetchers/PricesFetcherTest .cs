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
using TotovBuilder.Model.Items;
using TotovBuilder.Model.Test;
using TotovBuilder.Model.Utils;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Fetchers
{
    /// <summary>
    /// Represents tests on the <see cref="PricesFetcher"/> class.
    /// </summary>
    public class PricesFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnPrices()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.CreateForPricesAndBarters(TestData.PricesJson);

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            bartersFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.Barters)
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                CreateQuestsFetcher(apiRequesterMock.Object, configurationWrapperMock.Object),
                tarkovValuesFetcherMock.Object,
                CreateTradersFetcher(apiRequesterMock.Object, configurationWrapperMock.Object));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            List<Price> expected =
            [
                .. TestData.Prices.Concat(TestData.Barters),
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

            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Fetch_WithBartersOnlyUsingCurrency_ShouldTransformBarterToPrice()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.CreateForPricesAndBarters(@"{
  ""data"": {
    ""items"": {
      ""5d235b4d86f7742e017bc88a"": {
        ""id"": ""5d235b4d86f7742e017bc88a"",
        ""buyFromTrader"": [
          {
            ""trader"": ""6617beeaa9cfa777ca915b7c"",
            ""price"": 10000,
            ""priceRUB"": 10000,
            ""currency"": ""RUB"",
            ""minTraderLevel"": 1,
            ""taskUnlock"": null
          }
        ],
        ""types"": [
          ""noFlea""
        ]
      }
    },
    ""fleaMarket"": {
      ""enabled"": true,
      ""normalizedName"": ""flea-market""
    }
  }
}");

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            bartersFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns([new()
                    {
                        BarterItems =
                        [
                            new BarterItem()
                            {
                                ItemId = "5d235b4d86f7742e017bc88a",
                                Quantity = 6
                            }
                        ],
                        CurrencyName = "barter",
                        ItemId = "67f90180f07898267b0a4ed7",
                        Merchant = "ref",
                        MerchantLevel = 2
                    }])
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                CreateQuestsFetcher(apiRequesterMock.Object, configurationWrapperMock.Object),
                tarkovValuesFetcherMock.Object,
                CreateTradersFetcher(apiRequesterMock.Object, configurationWrapperMock.Object));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            List<Price> expected =
            [
                new()
                {
                    CurrencyName = "GPCOIN",
                    ItemId = "67f90180f07898267b0a4ed7",
                    Merchant = "ref",
                    MerchantLevel = 2,
                    Value = 6,
                    ValueInMainCurrency = 60000
                },
                new()
                {
                    CurrencyName = "RUB",
                    ItemId = "5d235b4d86f7742e017bc88a",
                    Merchant = "ref",
                    MerchantLevel = 1,
                    Value = 10000,
                    ValueInMainCurrency = 10000
                },
                new Price()
                {
                    CurrencyName = "RUB",
                    ItemId = "5449016a4bdc2d6f028b456f",
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Value = 1,
                    ValueInMainCurrency = 1
                }
            ];

            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Fetch_WithInvalidData_ShouldReturnOnlyValidData()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.CreateForPricesAndBarters(@"{
  ""data"": {
    ""items"": {
      ""invalid-item"": {
        ""invalid"": {}
      },
      ""5783c43d2459774bbe137486"": {
        ""id"": ""5783c43d2459774bbe137486"",
        ""buyFromTrader"": [],
        ""types"": [],
        ""avg24hPrice"": 18111
      }
    },
    ""fleaMarket"": {
      ""enabled"": true,
      ""normalizedName"": ""flea-market""
    }
  }
}");

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            bartersFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns([])
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                CreateQuestsFetcher(apiRequesterMock.Object, configurationWrapperMock.Object),
                tarkovValuesFetcherMock.Object,
                CreateTradersFetcher(apiRequesterMock.Object, configurationWrapperMock.Object));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new Price[]
            {
                new()
                {
                    CurrencyName = "RUB",
                    ItemId = "5783c43d2459774bbe137486",
                    Merchant = "flea-market",
                    Value = 18111,
                    ValueInMainCurrency = 18111
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
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Fetch_ShouldOnlyReturnFleaMarketPricesOfItemsSoldOnTheFleaMarket(bool isFleaMarketEnabled)
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.CreateForPricesAndBarters(@"{
  ""data"": {
    ""fleaMarket"": {
      ""enabled"": " + isFleaMarketEnabled.ToString().ToLowerInvariant() + @",
      ""normalizedName"": ""flea-market""
    },
    ""items"": {
      ""item-sold-on-flea-market"": {
        ""avg24hPrice"": 15000,
        ""buyFromTrader"": [],
        ""id"": ""item-sold-on-flea-market"",
        ""minLevelForFlea"": 20,
        ""types"": []
      },
      ""item-not-sold-on-flea-market"": {
        ""avg24hPrice"": 15000,
        ""buyFromTrader"": [],
        ""id"": ""item-not-sold-on-flea-market"",
        ""minLevelForFlea"": 20,
        ""types"": [
          ""noFlea""
        ]
      },
      ""item-without-average-price"": {
        ""avg24hPrice"": 0,
        ""buyFromTrader"": [],
        ""id"": ""item-without-average-price"",
        ""minLevelForFlea"": 20,
        ""types"": []
      },
      ""item-sold-by-unknown-trader"": {
        ""avg24hPrice"": 15000,
        ""buyFromTrader"": [
          {
            ""currency"": ""RUB"",
            ""minTraderLevel"": 1,
            ""price"": 10000,
            ""priceRUB"": 10000,
            ""taskUnlock"": null,
            ""trader"": ""unknown-trader""
          }
        ],
        ""id"": ""item-sold-by-unknown-trader"",
        ""minLevelForFlea"": 20,
        ""types"": []
      }
    }
  }
}");

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            bartersFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns([])
                .Verifiable();

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                CreateQuestsFetcher(apiRequesterMock.Object, configurationWrapperMock.Object),
                tarkovValuesFetcherMock.Object,
                CreateTradersFetcher(apiRequesterMock.Object, configurationWrapperMock.Object));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            List<Price> expected =
            [
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

            if (isFleaMarketEnabled)
            {
                expected.Add(new Price()
                {
                    CurrencyName = "RUB",
                    ItemId = "item-sold-on-flea-market",
                    Merchant = "flea-market",
                    MerchantLevel = 20,
                    Value = 15000,
                    ValueInMainCurrency = 15000
                });
            }

            // The item sold by an unknown trader is ignored
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Fetch_WithFailedTarkovValuesFetch_ShouldFail()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.CreateForPricesAndBarters(TestData.PricesJson);

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Barters - No data fetched.")))
                .Verifiable();

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                CreateQuestsFetcher(apiRequesterMock.Object, configurationWrapperMock.Object),
                tarkovValuesFetcherMock.Object,
                CreateTradersFetcher(apiRequesterMock.Object, configurationWrapperMock.Object));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Single().Message.Should().Be("Prices - No data fetched.");
        }

        [Fact]
        public async Task Fetch_WithFailedBartersFetch_ShouldReturnOnlyPrices()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.CreateForPricesAndBarters(TestData.PricesJson);

            Mock<ITarkovValuesFetcher> tarkovValuesFetcherMock = new();
            tarkovValuesFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Ok()))
                .Verifiable();
            tarkovValuesFetcherMock
                .SetupGet(m => m.FetchedData)
                .Returns(TestData.TarkovValues)
                .Verifiable();

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock
                .Setup(m => m.Fetch())
                .Returns(Task.FromResult(Result.Fail("Error")))
                .Verifiable();

            PricesFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<PricesFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                bartersFetcherMock.Object,
                CreateQuestsFetcher(apiRequesterMock.Object, configurationWrapperMock.Object),
                tarkovValuesFetcherMock.Object,
                CreateTradersFetcher(apiRequesterMock.Object, configurationWrapperMock.Object));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new List<Price>(TestData.Prices)
            {
                new()
                {
                    CurrencyName = "RUB",
                    ItemId = "5449016a4bdc2d6f028b456f",
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Value = 1,
                    ValueInMainCurrency = 1
                }
            });
        }

        private static QuestsFetcher CreateQuestsFetcher(IApiRequester apiRequester, IConfigurationWrapper configurationWrapper)
        {
            return new QuestsFetcher(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                "en",
                new Mock<ILogger<QuestsFetcher>>().Object,
                apiRequester,
                configurationWrapper);
        }

        private static TradersFetcher CreateTradersFetcher(IApiRequester apiRequester, IConfigurationWrapper configurationWrapper)
        {
            return new TradersFetcher(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                new Mock<ILogger<TradersFetcher>>().Object,
                apiRequester,
                configurationWrapper);
        }
    }
}
