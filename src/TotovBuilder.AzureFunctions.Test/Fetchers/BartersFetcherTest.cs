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
    /// Represents tests on the <see cref="BartersFetcher"/> class.
    /// </summary>
    public class BartersFetcherTest
    {
        [Fact]
        public async Task Fetch_ShouldReturnBarters()
        {
            // Arrange
            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.CreateForPricesAndBarters(bartersJson: TestData.BartersJson);

            BartersFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                new Mock<ILogger<BartersFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                CreateQuestsFetcher(apiRequesterMock.Object, configurationWrapperMock.Object),
                CreateTradersFetcher(apiRequesterMock.Object, configurationWrapperMock.Object));

            // Act
            Result result = await fetcher.Fetch();

            // Assert
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(TestData.Barters);
        }

        [Fact]
        public async Task Fetch_WithBartersRequiringObtainedItem_ShouldIgnoreThoseBarters()
        {
            // Arrange
            string bartersJson = @"{
  ""data"": [
    {
      ""id"": ""test-barter-1"",
      ""trader"": ""5a7c2eca46aef81a7ca2145d"",
      ""taskUnlock"": null,
      ""minTraderLevel"": 3,
      ""requiredItems"": [
        {
          ""item"": ""545cdb794bdc2d3a198b456a"",
          ""count"": 2
        }
      ],
      ""offeredItem"": {
        ""item"": ""545cdb794bdc2d3a198b456a"",
        ""count"": 1
      }
    },
    {
      ""id"": ""test-barter-2"",
      ""trader"": ""5a7c2eca46aef81a7ca2145d"",
      ""taskUnlock"": null,
      ""minTraderLevel"": 2,
      ""requiredItems"": [
        {
          ""item"": ""5448be9a4bdc2dfd2f8b456a"",
          ""count"": 1
        }
      ],
      ""offeredItem"": {
        ""item"": ""545cdb794bdc2d3a198b456a"",
        ""count"": 1
      }
    }
  ]
}";

            Mock<IConfigurationWrapper> configurationWrapperMock = new();
            configurationWrapperMock
                .SetupGet(m => m.Values)
                .Returns(TestData.AzureFunctionsConfiguration)
                .Verifiable();

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.CreateForPricesAndBarters(bartersJson: bartersJson);

            BartersFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                new Mock<ILogger<BartersFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                CreateQuestsFetcher(apiRequesterMock.Object, configurationWrapperMock.Object),
                CreateTradersFetcher(apiRequesterMock.Object, configurationWrapperMock.Object));

            // Act
            Result result = await fetcher.Fetch();

            // Assert            
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(
                new Price[]
                {
                    new()
                    {
                        BarterItems =
                        [
                            new BarterItem()
                            {
                                ItemId = "5448be9a4bdc2dfd2f8b456a",
                                Quantity = 1
                            }
                        ],
                        CurrencyName = "barter",
                        ItemId = "545cdb794bdc2d3a198b456a",
                        Merchant = "mechanic",
                        MerchantLevel = 2
                    }
                });
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

            Mock<IApiRequester> apiRequesterMock = ApiRequesterMock.CreateForPricesAndBarters(bartersJson: @"{
  ""data"": [
    {
      ""invalid"": {}
    },
    {
      ""id"": ""test-barter-1"",
      ""trader"": ""54cb50c76803fa8b248b4571"",
      ""taskUnlock"": ""59675d6c86f7740a842fc482"",
      ""minTraderLevel"": 1,
      ""requiredItems"": [
        {
          ""item"": ""5e32f56fcb6d5863cc5e5ee4"",
          ""count"": 2
        },
        {
          ""item"": ""5b432be65acfc433000ed01f"",
          ""count"": 1
        }
      ],
      ""offeredItem"": {
        ""item"": ""545cdb794bdc2d3a198b456a"",
        ""count"": 1
      }
    }
  ]
}");

            BartersFetcher fetcher = new(
                new GameMode()
                {
                    ApiQueryValue = "regular",
                    Name = "pvp"
                },
                new Mock<ILogger<BartersFetcher>>().Object,
                apiRequesterMock.Object,
                configurationWrapperMock.Object,
                CreateQuestsFetcher(apiRequesterMock.Object, configurationWrapperMock.Object),
                CreateTradersFetcher(apiRequesterMock.Object, configurationWrapperMock.Object));

            // Act
            Result result = await fetcher.Fetch();

            // Assert            
            result.IsSuccess.Should().BeTrue();
            fetcher.FetchedData.Should().BeEquivalentTo(new Price[]
            {
                new()
                {
                    BarterItems =
                    [
                        new BarterItem()
                        {
                            ItemId = "5e32f56fcb6d5863cc5e5ee4",
                            Quantity = 2
                        },
                        new BarterItem()
                        {
                            ItemId = "5b432be65acfc433000ed01f",
                            Quantity = 1
                        }
                    ],
                    CurrencyName = "barter",
                    ItemId = "545cdb794bdc2d3a198b456a",
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Quest = new Quest()
                    {
                        Id = "59675d6c86f7740a842fc482",
                        Name = "Ice Cream Cones",
                        WikiLink = "https://escapefromtarkov.fandom.com/wiki/Ice_Cream_Cones"
                    }
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
