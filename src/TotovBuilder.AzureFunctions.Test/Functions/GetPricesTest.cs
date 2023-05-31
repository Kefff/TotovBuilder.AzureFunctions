using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions;
using TotovBuilder.AzureFunctions.Abstractions.Fetchers;
using TotovBuilder.AzureFunctions.Functions;
using TotovBuilder.AzureFunctions.Test.Mocks;
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
            List<Price> barters = new() // Not using TestData.Barters here because this tests modifies the list making other tests fail
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

            List<Price> prices = new() // Not using TestData.Prices here because this tests modifies the list making other tests fail
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

            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();

            Mock<IHttpResponseDataFactory> httpResponseDataFactoryMock = new();
            httpResponseDataFactoryMock
                .Setup(m => m.CreateEnumerableResponse(It.IsAny<HttpRequestData>(), It.IsAny<IEnumerable<object>>()))
                .Returns(Task.FromResult((HttpResponseData)new Mock<HttpResponseDataImplementation>().Object));

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Price>?>(barters));

            Mock<IPricesFetcher> pricesFetcherMock = new();
            pricesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Price>?>(prices));

            GetPrices function = new(
                azureFunctionsConfigurationReaderMock.Object,
                httpResponseDataFactoryMock.Object,
                bartersFetcherMock.Object,
                pricesFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new HttpRequestDataImplementation());

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateEnumerableResponse(
                It.IsAny<HttpRequestData>(),
                It.Is<IEnumerable<Price>>(v => v.Should().BeEquivalentTo(
                    new Price[]
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
                    },
                    string.Empty,
                    Array.Empty<object>())
                != null)));
        }

        [Fact]
        public async Task Run_WithoutData_ShouldReturnEmptyResponse()
        {
            // Arrange
            Mock<IAzureFunctionsConfigurationReader> azureFunctionsConfigurationReaderMock = new();

            Mock<IHttpResponseDataFactory> httpResponseDataFactoryMock = new();
            httpResponseDataFactoryMock
                .Setup(m => m.CreateEnumerableResponse(It.IsAny<HttpRequestData>(), It.IsAny<IEnumerable<object>>()))
                .Returns(Task.FromResult((HttpResponseData)new Mock<HttpResponseDataImplementation>().Object));

            Mock<IBartersFetcher> bartersFetcherMock = new();
            bartersFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Price>?>(null));

            Mock<IPricesFetcher> pricesFetcherMock = new();
            pricesFetcherMock.Setup(m => m.Fetch()).Returns(Task.FromResult<IEnumerable<Price>?>(null));

            GetPrices function = new(
                azureFunctionsConfigurationReaderMock.Object,
                httpResponseDataFactoryMock.Object,
                bartersFetcherMock.Object,
                pricesFetcherMock.Object);

            // Act
            HttpResponseData result = await function.Run(new HttpRequestDataImplementation());

            // Assert
            azureFunctionsConfigurationReaderMock.Verify(m => m.Load());
            httpResponseDataFactoryMock.Verify(m => m.CreateEnumerableResponse(It.IsAny<HttpRequestData>(), Array.Empty<Item>()));
        }
    }
}
