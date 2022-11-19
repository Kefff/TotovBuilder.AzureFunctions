using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.Model.Configuration;
using TotovBuilder.Model.Items;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GetPrices"/> class.
    /// </summary>
    public class GetPricesTest
    {
        [Fact]
        public async Task Run_ShouldFetchData()
        {
            // Arrange
            List<Price> barters = new List<Price>() // Not using TestData.Barters here because this tests modifies the list making other tests fail
            {
                new Price()
                {
                    BarterItems = new BarterItem[]
                    {
                        new BarterItem()
                        {
                            ItemId = "5f9949d869e2777a0e779ba5",
                            Quantity = 4
                        }
                    },
                    CurrencyName = "barter",
                    ItemId = "545cdb794bdc2d3a198b456a",
                    Merchant = "mechanic",
                    MerchantLevel = 3
                },
                new Price()
                {
                    BarterItems = new BarterItem[]
                    {
                        new BarterItem()
                        {
                            ItemId = "5c13cd2486f774072c757944",
                            Quantity = 5
                        }
                    },
                    ItemId = "57dc2fa62459775949412633"
                }
            };

            List<Price> prices = new List<Price>() // Not using TestData.Prices here because this tests modifies the list making other tests fail
            {
                new Price()
                {
                    CurrencyName = "RUB",
                    ItemId = "57dc2fa62459775949412633",
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Quest = new Quest()
                    {
                        Id = "5936d90786f7742b1420ba5b",
                        Name = "Debut",
                        WikiLink = "https://escapefromtarkov.fandom.com/wiki/Debut",
                    },
                    Value = 24605,
                    ValueInMainCurrency = 24605
                },
                new Price()
                {
                    CurrencyName = "RUB",
                    ItemId = "57dc2fa62459775949412633",
                    Merchant = "flea-market",
                    Value = 35000,
                    ValueInMainCurrency = 35000
                }
            };

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();

            Mock<IBartersFetcher> bartersFetcherMock = new Mock<IBartersFetcher>();
            bartersFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Price>?>(barters));

            Mock<IPricesFetcher> pricesFetcherMock = new Mock<IPricesFetcher>();
            pricesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Price>?>(prices));

            GetPrices function = new GetPrices(azureFunctionsConfigurationReaderMock.Object, bartersFetcherMock.Object, pricesFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(new List<Price>()
            {
                new Price()
                {
                    CurrencyName = "RUB",
                    ItemId = "57dc2fa62459775949412633",
                    Merchant = "prapor",
                    MerchantLevel = 1,
                    Quest = new Quest()
                    {
                        Id = "5936d90786f7742b1420ba5b",
                        Name = "Debut",
                        WikiLink = "https://escapefromtarkov.fandom.com/wiki/Debut"
                    },
                    Value = 24605,
                    ValueInMainCurrency = 24605
                },
                new Price()
                {
                    CurrencyName = "RUB",
                    ItemId = "57dc2fa62459775949412633",
                    Merchant = "flea-market",
                    Value = 35000,
                    ValueInMainCurrency = 35000
                },
                new Price()
                {
                    BarterItems = new BarterItem[]
                    {
                        new BarterItem()
                        {
                            ItemId = "5c13cd2486f774072c757944",
                            Quantity = 5
                        },
                    },
                    ItemId = "57dc2fa62459775949412633"
                },
                new Price()
                {
                    BarterItems = new BarterItem[]
                    {
                        new BarterItem()
                        {
                            ItemId = "5f9949d869e2777a0e779ba5",
                            Quantity = 4
                        }
                    },
                    CurrencyName = "barter",
                    ItemId = "545cdb794bdc2d3a198b456a",
                    Merchant = "mechanic",
                    MerchantLevel = 3
                },
            });
        }

        [Fact]
        public async Task Run_WithBartersRequiringObtainedItem_ShouldIgnoreThoseBarters()
        {
            // Arrange
            List<Price> barters = new List<Price>() // Not using TestData.Barters here because this tests modifies the list making other tests fail
            {
                new Price()
                {
                    BarterItems = new BarterItem[]
                    {
                        new BarterItem()
                        {
                            ItemId = "545cdb794bdc2d3a198b456a",
                            Quantity = 2
                        }
                    },
                    CurrencyName = "barter",
                    ItemId = "545cdb794bdc2d3a198b456a",
                    Merchant = "mechanic",
                    MerchantLevel = 3
                },
                new Price()
                {
                    BarterItems = new BarterItem[]
                    {
                        new BarterItem()
                        {
                            ItemId = "5448be9a4bdc2dfd2f8b456a",
                            Quantity = 1
                        }
                    },
                    CurrencyName = "barter",
                    ItemId = "545cdb794bdc2d3a198b456a",
                    Merchant = "mechanic",
                    MerchantLevel = 2
                }
            };

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();

            Mock<IBartersFetcher> bartersFetcherMock = new Mock<IBartersFetcher>();
            bartersFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Price>?>(barters));

            Mock<IPricesFetcher> pricesFetcherMock = new Mock<IPricesFetcher>();
            pricesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Price>?>(null));

            GetPrices function = new GetPrices(azureFunctionsConfigurationReaderMock.Object, bartersFetcherMock.Object, pricesFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(new List<Price>()
            {
                new Price()
                {
                    BarterItems = new BarterItem[]
                    {
                        new BarterItem()
                        {
                            ItemId = "5448be9a4bdc2dfd2f8b456a",
                            Quantity = 1
                        }
                    },
                    CurrencyName = "barter",
                    ItemId = "545cdb794bdc2d3a198b456a",
                    Merchant = "mechanic",
                    MerchantLevel = 2
                }
            });
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new Mock<IAzureFunctionsConfigurationReader>();

            Mock<IBartersFetcher> bartersFetcherMock = new Mock<IBartersFetcher>();
            bartersFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Price>?>(null));

            Mock<IPricesFetcher> pricesFetcherMock = new Mock<IPricesFetcher>();
            pricesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Price>?>(null));

            GetPrices function = new GetPrices(azureFunctionsConfigurationReaderMock.Object, bartersFetcherMock.Object, pricesFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(Array.Empty<Item>());
        }
    }
}
