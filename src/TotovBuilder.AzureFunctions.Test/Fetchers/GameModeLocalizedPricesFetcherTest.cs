using System.Linq;
using System.Net;
using System.Net.Http;
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

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async (HttpRequestMessage hrm) =>
                {
                    string requestContent = await hrm.Content!.ReadAsStringAsync();
                    string responseContent;

                    if (requestContent.Contains("gameMode: regular"))
                    {
                        responseContent = requestContent.Contains("lang: en") ? PricesJsonPvpEN : PricesJsonPvpFR;
                    }
                    else
                    {
                        responseContent = requestContent.Contains("lang: en") ? PricesJsonPveEN : PricesJsonPveFR;
                    }

                    HttpResponseMessage response = new(HttpStatusCode.OK)
                    {
                        Content = new StringContent(responseContent)
                    };

                    return response;
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                    .Returns(httpClientWrapperMock.Object)
                    .Verifiable();

            Mock<IBartersFetcher> barterFetcherMock = new();
            barterFetcherMock
                .Setup(m => m.Fetch())
                    .Returns(Task.FromResult(Result.Ok()))
                    .Verifiable();
            barterFetcherMock
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

            GameModeLocalizedPricesFetcher gameModeLocalizedPricesFetcher = new(
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                barterFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

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

            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Exactly(4));
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

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(async (HttpRequestMessage hrm) =>
                {
                    string requestContent = await hrm.Content!.ReadAsStringAsync();
                    string responseContent;

                    if (requestContent.Contains("gameMode: regular"))
                    {
                        responseContent = requestContent.Contains("lang: en") ? PricesJsonPvpEN : PricesJsonPvpFR;
                    }
                    else
                    {
                        responseContent = requestContent.Contains("lang: en") ? PricesJsonPveEN : PricesJsonPveFR;
                    }

                    HttpResponseMessage response = new(HttpStatusCode.OK)
                    {
                        Content = new StringContent(responseContent)
                    };

                    return response;
                })
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IBartersFetcher> barterFetcherMock = new();
            barterFetcherMock
                .Setup(m => m.Fetch())
                    .Returns(Task.FromResult(Result.Ok()))
                    .Verifiable();
            barterFetcherMock
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

            GameModeLocalizedPricesFetcher gameModeLocalizedPricesFetcher = new(
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                barterFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

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

            httpClientWrapperMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Exactly(4));
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

            Mock<IHttpClientWrapper> httpClientWrapperMock = new();
            httpClientWrapperMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@$"{{
  ""data"": {{
    ""items"": []
  }}
}}")
                }))
                .Verifiable();

            Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock = new();
            httpClientWrapperFactoryMock
                .Setup(m => m.Create())
                .Returns(httpClientWrapperMock.Object)
                .Verifiable();

            Mock<IBartersFetcher> barterFetcherMock = new();
            barterFetcherMock
                .Setup(m => m.Fetch())
                    .Returns(Task.FromResult(Result.Ok()))
                    .Verifiable();
            barterFetcherMock
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

            GameModeLocalizedPricesFetcher gameModeLocalizedPricesFetcher = new(
                new Mock<ILogger<PricesFetcher>>().Object,
                httpClientWrapperFactoryMock.Object,
                configurationWrapperMock.Object,
                barterFetcherMock.Object,
                tarkovValuesFetcherMock.Object);

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

        private const string PricesJsonPvpEN = @$"{{
  ""data"": {{
    ""items"": [
      {{
        ""buyFor"": [
          {{
            ""currency"": ""USD"",
            ""price"": 539,
            ""priceRUB"": 64141,
            ""vendor"": {{
              ""minTraderLevel"": 2,
              ""taskUnlock"": {{
                ""id"": ""5a27b80086f774429a5d7e20"",
                ""name"": ""Eagle Eye"",
                ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Eagle_Eye""
              }},
              ""trader"": {{
                ""normalizedName"": ""peacekeeper""
              }}
            }}
          }}
        ],
        ""id"": ""58948c8e86f77409493f7266""
      }}
    ]
  }}
}}";

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

        private const string PricesJsonPvpFR = @$"{{
  ""data"": {{
    ""items"": [
      {{
        ""buyFor"": [
          {{
            ""currency"": ""USD"",
            ""price"": 539,
            ""priceRUB"": 64141,
            ""vendor"": {{
              ""minTraderLevel"": 2,
              ""taskUnlock"": {{
                ""id"": ""5a27b80086f774429a5d7e20"",
                ""name"": ""Oeil d'aigle"",
                ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Eagle_Eye""
              }},
              ""trader"": {{
                ""normalizedName"": ""peacekeeper""
              }}
            }}
          }}
        ],
        ""id"": ""58948c8e86f77409493f7266""
      }}
    ]
  }}
}}";

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

        private const string PricesJsonPveEN = @$"{{
  ""data"": {{
    ""items"": [
      {{
        ""buyFor"": [
          {{
            ""currency"": ""USD"",
            ""price"": 270,
            ""priceRUB"": 32070,
            ""vendor"": {{
              ""minTraderLevel"": 2,
              ""taskUnlock"": {{
                ""id"": ""5a27b80086f774429a5d7e20"",
                ""name"": ""Eagle Eye"",
                ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Eagle_Eye""
              }},
              ""trader"": {{
                ""normalizedName"": ""peacekeeper""
              }}
            }}
          }}
        ],
        ""id"": ""58948c8e86f77409493f7266""
      }}
    ]
  }}
}}";

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

        private const string PricesJsonPveFR = @$"{{
  ""data"": {{
    ""items"": [
      {{
        ""buyFor"": [
          {{
            ""currency"": ""USD"",
            ""price"": 270,
            ""priceRUB"": 32070,
            ""vendor"": {{
              ""minTraderLevel"": 2,
              ""taskUnlock"": {{
                ""id"": ""5a27b80086f774429a5d7e20"",
                ""name"": ""Oeil d'aigle"",
                ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Eagle_Eye""
              }},
              ""trader"": {{
                ""normalizedName"": ""peacekeeper""
              }}
            }}
          }}
        ],
        ""id"": ""58948c8e86f77409493f7266""
      }}
    ]
  }}
}}";
    }
}
