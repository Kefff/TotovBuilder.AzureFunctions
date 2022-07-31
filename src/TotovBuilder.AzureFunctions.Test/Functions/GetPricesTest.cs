using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TotovBuilder.AzureFunctions.Abstraction.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Models;
using TotovBuilder.AzureFunctions.Models.Items;
using TotovBuilder.AzureFunctions.Test.Mocks;
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
            List<Item> barters = new List<Item>() // Not using TestData.Barters here because this tests modifies the list making other tests fail
            {
                new Item()
                {
                    Id = "57dc2fa62459775949412633",
                    Prices = new Price[]
                    {
                        new Price()
                        {
                            BarterItems = new BarterItem[]
                            {
                                new BarterItem()
                                {
                                    ItemId = "5c13cd2486f774072c757944",
                                    Quantity = 5
                                }
                            }
                        }
                    }
                }
            };

            List<Item> prices = new List<Item>() // Not using TestData.Prices here because this tests modifies the list making other tests fail
            {
                new Item()
                {
                    Id = "57dc2fa62459775949412633",
                    Prices = new Price[]
                    {
                        new Price()
                        {
                            CurrencyName = "RUB",
                            Merchant = "prapor",
                            MerchantLevel = 1,
                            QuestId = "5936d90786f7742b1420ba5b",
                            Value = 24605,
                            ValueInMainCurrency = 24605
                        },
                        new Price()
                        {
                            CurrencyName = "RUB",
                            Merchant = "flea-market",
                            Value = 35000,
                            ValueInMainCurrency = 35000
                        }
                    }
                }
            };

            Mock<IBartersFetcher> bartersFetcherMock = new Mock<IBartersFetcher>();
            bartersFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Item>?>(barters));

            Mock<IPricesFetcher> pricesFetcherMock = new Mock<IPricesFetcher>();
            pricesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Item>?>(prices));

            GetPrices function = new GetPrices(bartersFetcherMock.Object, pricesFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(new List<Item>()
            {
                new Item()
                {
                    Id = "57dc2fa62459775949412633",
                    Prices = new Price[]
                    {
                        new Price()
                        {
                            CurrencyName = "RUB",
                            Merchant = "prapor",
                            MerchantLevel = 1,
                            QuestId = "5936d90786f7742b1420ba5b",
                            Value = 24605,
                            ValueInMainCurrency = 24605
                        },
                        new Price()
                        {
                            CurrencyName = "RUB",
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
                                }
                            }
                        }
                    }
                }
            });
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<IBartersFetcher> bartersFetcherMock = new Mock<IBartersFetcher>();
            bartersFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Item>?>(null));

            Mock<IPricesFetcher> pricesFetcherMock = new Mock<IPricesFetcher>();
            pricesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Item>?>(null));

            GetPrices function = new GetPrices(bartersFetcherMock.Object, pricesFetcherMock.Object);

            // Act
            IActionResult result = await function.Run(new Mock<HttpRequest>().Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeEquivalentTo(Array.Empty<Item>());
        }
    }
}
